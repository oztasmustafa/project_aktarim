using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InvoiceTransferApp.Core.DTO;
using InvoiceTransferApp.Core.Entities;
using InvoiceTransferApp.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InvoiceTransferApp.Service.Services
{
    public class NetsisRestService : INetsisRestService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _cachedToken = null;
        private static DateTime _tokenExpiry = DateTime.MinValue;

        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _grantType;
        private readonly string _branchCode;
        private readonly string _dbName;
        private readonly string _dbUser;
        private readonly string _dbPassword;
        private readonly string _dbType;

        public NetsisRestService()
        {
            _baseUrl = ConfigurationManager.AppSettings["NetsisRestUrl"] ?? "http://135.181.90.228:6969";
            _username = ConfigurationManager.AppSettings["NetsisRestUsername"] ?? "";
            _password = ConfigurationManager.AppSettings["NetsisRestPassword"] ?? "";
            _grantType = ConfigurationManager.AppSettings["NetsisRestGrantType"] ?? "password";
            _branchCode = ConfigurationManager.AppSettings["NetsisRestBranchCode"] ?? "0";
            _dbName = ConfigurationManager.AppSettings["NetsisRestDbName"] ?? "";
            _dbUser = ConfigurationManager.AppSettings["NetsisRestDbUser"] ?? "";
            _dbPassword = ConfigurationManager.AppSettings["NetsisRestDbPassword"] ?? "";
            _dbType = ConfigurationManager.AppSettings["NetsisRestDbType"] ?? "0";

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public SaveItemSlipResult SaveItemSlip(Fatura fatura)
        {
            return SaveItemSlipAsync(fatura).GetAwaiter().GetResult();
        }

        /// <summary>CCP uyumlu: api/v2/revoke GET, Bearer token.</summary>
        public bool RevokeToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            try
            {
                var url = $"{_baseUrl.TrimEnd('/')}/api/v2/revoke";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Bearer " + token);
                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<SaveItemSlipResult> SaveItemSlipAsync(Fatura fatura)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    WriteResponseToFile("Token alınamadı!");
                    System.Diagnostics.Debug.WriteLine("[Netsis] Token alınamadı! Detay: NetsisToken.txt");
                    return new SaveItemSlipResult { Success = false, ErrorMessage = "Token alınamadı. Detay için NetsisToken.txt dosyasına bakın." };
                }

                var request = MapToItemSlipsRequest(fatura);
                // SIPDEPOKODKULLAN false olsa bile JSON'da görünmeli (DefaultValueHandling.Include)
                var jsonRequest = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    Formatting = Formatting.Indented,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                });

                WriteRequestToFile(jsonRequest);

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v2/ItemSlips")
                {
                    Content = content
                };
                requestMessage.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsStringAsync();

                WriteResponseToFile($"HTTP {(int)response.StatusCode}\n{responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    return new SaveItemSlipResult
                    {
                        Success = false,
                        ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}"
                    };
                }

                var result = JObject.Parse(responseBody);
                bool isSuccessful = result["IsSuccessful"]?.Value<bool>() ?? result["isSuccessful"]?.Value<bool>() ?? false;
                string errorDesc = (result["ErrorDesc"] ?? result["errorDesc"])?.Value<string>() ?? "";

                if (!isSuccessful)
                {
                    WriteResponseToFile($"API Hatası: {errorDesc}");
                    return new SaveItemSlipResult { Success = false, ErrorMessage = errorDesc };
                }

                // API'den dönen fiş/fatura numarası (farklı property isimleri denenir)
                string refNo = result.SelectToken("Data.FisNo")?.Value<string>()
                    ?? result.SelectToken("Data.KayitNo")?.Value<string>()
                    ?? result.SelectToken("Data.ReferenceNumber")?.Value<string>()
                    ?? result["FisNo"]?.Value<string>()
                    ?? result["KayitNo"]?.Value<string>();
                return new SaveItemSlipResult
                {
                    Success = true,
                    ReferenceNumber = !string.IsNullOrWhiteSpace(refNo) ? refNo : $"REST-{DateTime.Now:yyyyMMddHHmmss}"
                };
            }
            catch (Exception ex)
            {
                WriteResponseToFile($"Exception: {ex.Message}\n{ex.StackTrace}");
                return new SaveItemSlipResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.Now < _tokenExpiry)
            {
                return _cachedToken;
            }

            // CCP LogoRestService ile aynı URL: base + "api/v2/token"
            var tokenUrl = $"{_baseUrl.TrimEnd('/')}/api/v2/token";

            try
            {
                // CCP ile aynı sıra: grant_type, branchcode, password, username, dbname, dbuser, dbpassword, dbtype
                var formPairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", _grantType ?? "password"),
                    new KeyValuePair<string, string>("branchcode", string.IsNullOrEmpty(_branchCode) ? "0" : _branchCode),
                    new KeyValuePair<string, string>("password", _password ?? ""),
                    new KeyValuePair<string, string>("username", _username ?? ""),
                    new KeyValuePair<string, string>("dbname", _dbName ?? ""),
                    new KeyValuePair<string, string>("dbuser", _dbUser ?? ""),
                    new KeyValuePair<string, string>("dbpassword", _dbPassword ?? ""),
                    new KeyValuePair<string, string>("dbtype", _dbType ?? "0")
                };

                WriteTokenToFile($"POST {tokenUrl}\nCCP sırası: grant_type, branchcode, password, username, dbname, dbuser, dbpassword, dbtype");
                var content = new FormUrlEncodedContent(formPairs);
                var response = await _httpClient.PostAsync(tokenUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    WriteTokenToFile($"Token HTTP Error: {(int)response.StatusCode} {response.StatusCode}\nBody: {(string.IsNullOrEmpty(responseBody) ? "(boş)" : responseBody)}");
                    return null;
                }

                // CCP gibi access_token parse (biz JSON ile alıyoruz)
                if (responseBody.IndexOf("access_token", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var tokenData = JObject.Parse(responseBody);
                    _cachedToken = tokenData["access_token"]?.Value<string>();
                    var expiresIn = tokenData["expires_in"]?.Value<int>() ?? 3600;
                    _tokenExpiry = DateTime.Now.AddSeconds(expiresIn - 60);
                    WriteTokenToFile($"Token alındı. Expires: {_tokenExpiry}");
                    return _cachedToken;
                }

                WriteTokenToFile($"Yanıtta access_token yok.\nBody: {responseBody}");
                return null;
            }
            catch (Exception ex)
            {
                WriteTokenToFile($"Token Exception: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private static ItemSlipsRequestDto MapToItemSlipsRequest(Fatura fatura)
        {
            var u = fatura.FatUst;

            return new ItemSlipsRequestDto
            {
                SIPDEPOKODKULLAN = 1,
                Seri = string.IsNullOrWhiteSpace(fatura.Seri) ? "F" : fatura.Seri,
                KayitliNumaraOtomatikGuncellensin = true,
                SeriliHesapla = true,
                FatUst = new ItemSlipsFatUstDto
                {
                    Sube_Kodu = u.Sube_Kodu,
                    CariKod = u.CariKod ?? "",
                    Tarih = u.Tarih,
                    FIYATTARIHI = u.FIYATTARIHI,
                    Tip = u.Tip,
                    TIPI = u.TIPI,
                    Proje_Kodu = string.IsNullOrWhiteSpace(u.Proje_Kodu) ? null : u.Proje_Kodu
                },
                Kalems = fatura.Kalems.Select(k =>
                {
                    return new ItemSlipsKalemDto
                    {
                        StokKodu = k.StokKodu ?? "",
                        STra_GCMIK = k.STra_GCMIK,
                        STra_NF = double.TryParse(k.STra_NF?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var nf) ? nf : 0,
                        STra_BF = double.TryParse(k.STra_BF?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var bf) ? bf : 0,
                        STra_ACIKLAMA = k.STra_ACIK ?? "",
                        STra_DOVTIP = "0",
                        STra_DOVFIAT = 0,
                        DEPO_KODU = k.DEPO_KODU,
                        ReferansKodu = "",
                        ProjeKodu = k.ProjeKodu ?? ""
                    };
                }).ToList()
            };
        }

        private static void WriteTokenToFile(string content)
        {
            try
            {
                var full = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{content}";
                File.WriteAllText("NetsisToken.txt", full);
                System.Diagnostics.Debug.WriteLine($"[Netsis Token] {content}");
            }
            catch { }
        }

        private static void WriteRequestToFile(string json)
        {
            try
            {
                File.WriteAllText("NetsisItemSlipsRequest.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{json}");
            }
            catch { }
        }

        private static void WriteResponseToFile(string content)
        {
            try
            {
                var full = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{content}";
                File.WriteAllText("NetsisItemSlipsResponse.txt", full);
                System.Diagnostics.Debug.WriteLine($"[Netsis ItemSlips Response] {content}");
            }
            catch { }
        }
    }
}
