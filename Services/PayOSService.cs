using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace B_M.Services
{
    public class PayOSService
    {
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _baseUrl;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;
        private readonly string _webhookUrl;

        public PayOSService()
        {
            _clientId = ConfigurationManager.AppSettings["PayOSClientId"];
            _apiKey = ConfigurationManager.AppSettings["PayOSApiKey"];
            _checksumKey = ConfigurationManager.AppSettings["PayOSChecksumKey"];
            _baseUrl = ConfigurationManager.AppSettings["PayOSApiBaseUrl"] ?? "https://api.payos.vn/v2/";
            _returnUrl = ConfigurationManager.AppSettings["PayOSReturnUrl"];
            _cancelUrl = ConfigurationManager.AppSettings["PayOSCancelUrl"];
            _webhookUrl = ConfigurationManager.AppSettings["PayOSWebhookUrl"];
        }

        // Get PayOS credentials
        public PayOSCredentials GetCredentials()
        {
            return new PayOSCredentials
            {
                ClientId = _clientId,
                ApiKey = _apiKey,
                ChecksumKey = _checksumKey,
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl,
                WebhookUrl = _webhookUrl
            };
        }

        // Create payment link with items
        public PayOSCreateLinkResponse CreatePaymentLink(decimal amount, long orderCode, List<PaymentItem> items, string desc)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var requestObj = new
                {
                    orderCode = orderCode,
                    amount = (int)(amount),
                    description = desc,
                    items = items,
                    returnUrl = _returnUrl,
                    cancelUrl = _cancelUrl,
                    expiredAt = (int)(DateTime.UtcNow.AddMinutes(5).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) // 1 hour expiry
                };

                // Serialize with camelCase naming
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                
                var jsonData = JObject.Parse(JsonConvert.SerializeObject(requestObj, settings));
             
                // Generate signature for PayOS
                var signature = GenerateSignature(jsonData);
                jsonData["signature"] = signature;
                
                var jsonContent = jsonData.ToString();
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync(_baseUrl + "payment-requests", content).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var payOSResponse = JsonConvert.DeserializeObject<PayOSCreateLinkResponse>(responseContent);
                    return payOSResponse;
                }
                else
                {
                    throw new Exception($"PayOS API Error: {response.StatusCode} - {responseContent}");
                }
            }
        }

        // Get payment information from PayOS
        public PayOSPaymentInfo GetPaymentInfo(string paymentLinkId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = httpClient.GetAsync(_baseUrl + $"payment-requests/{paymentLinkId}").Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var payOSResponse = JsonConvert.DeserializeObject<PayOSPaymentInfo>(responseContent);
                    
                    //// Verify signature using only the Data object
                    //if (!string.IsNullOrEmpty(payOSResponse.Signature) && !string.IsNullOrEmpty(_checksumKey))
                    //{
                    //    // Get data object from original JSON response
                    //    var respJson = JObject.Parse(responseContent);
                    //    var dataToken = respJson["data"];
                        
                    //    if (dataToken != null)
                    //    {
                    //        // Convert data to dictionary recursively (flatten nested objects and arrays)
                    //        var dataDict = ConvertJTokenToDictionary(dataToken);
                            
                    //        // Generate signature from Data object only
                    //        var calculatedSignature = _cryptoProvider.CreateSignatureFromObject(dataDict, _checksumKey);
                            
                    //        // Verify signature
                    //        if (!calculatedSignature.Equals(payOSResponse.Signature, StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            throw new Exception("Invalid signature from PayOS");
                    //        }
                    //    }
                    //}
                    
                    return payOSResponse;
                }
                else
                {
                    throw new Exception($"PayOS API Error: {response.StatusCode} - {responseContent}");
                }
            }
        }
        
        // Verify payment status
        public bool VerifyPaymentStatus(string paymentLinkId)
        {
            try
            {
                var paymentInfo = GetPaymentInfo(paymentLinkId);
                return paymentInfo != null && 
                       paymentInfo.Code == "00" && 
                       paymentInfo.Data != null && 
                       paymentInfo.Data.Status == "PAID";
            }
            catch
            {
                return false;
            }
        }

        // Cancel payment link
        public void CancelPaymentLink(string paymentLinkId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = httpClient.DeleteAsync(_baseUrl + $"payment-requests/{paymentLinkId}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"PayOS API Error: {response.StatusCode} - {responseContent}");
                }
            }
        }

        // Crypto provider for signature generation
        private CryptoProvider _cryptoProvider = new CryptoProvider();
        
        // Public access to crypto provider and keys
        public CryptoProvider GetCryptoProvider()
        {
            return _cryptoProvider;
        }
        
        public string GetChecksumKey()
        {
            return _checksumKey;
        }

        // Generate signature using HMAC-SHA256 for payment request
        private string GenerateSignature(JObject jsonData)
        {
            // Convert JObject to Dictionary
            var dataDict = _cryptoProvider.ConvertJObjectToDictionary(jsonData);
            
            // Create payment request signature with specific fields
            return _cryptoProvider.CreatePaymentRequestSignature(dataDict, _checksumKey);
        }

        // Verify webhook signature
        public bool VerifySignature(string dataJson, string signature)
        {
            try
            {
                var jsonData = JObject.Parse(dataJson);
                
                // Remove signature field from data
                var dataToSign = new JObject(jsonData);
                dataToSign.Remove("signature");
                
                // Convert to dictionary
                var dataDict = _cryptoProvider.ConvertJObjectToDictionary(dataToSign);
                
                // Create signature from object
                var calculatedSignature = _cryptoProvider.CreateSignatureFromObject(dataDict, _checksumKey);
                
                return calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to convert JToken to Dictionary (recursive conversion of nested objects and arrays)
        /// </summary>
        private static Dictionary<string, object> ConvertJTokenToDictionary(JToken token)
        {
            var dict = new Dictionary<string, object>();

            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in ((JObject)token).Properties())
                {
                    dict[prop.Name] = ConvertJTokenValue(prop.Value);
                }
            }

            return dict;
        }

        private static object ConvertJTokenValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ConvertJTokenToDictionary(token);
                case JTokenType.Array:
                    var list = new List<object>();
                    foreach (var item in token)
                    {
                        list.Add(ConvertJTokenValue(item));
                    }
                    return list;
                case JTokenType.String:
                    return token.ToString();
                case JTokenType.Integer:
                    return token.ToObject<long>();
                case JTokenType.Float:
                    return token.ToObject<decimal>();
                case JTokenType.Boolean:
                    return token.ToObject<bool>();
                case JTokenType.Null:
                    return null;
                default:
                    return token.ToString();
            }
        }
    }

    // PayOS Models
    public class PaymentItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty("price")]
        public int Price { get; set; }
    }

    public class PayOSCredentials
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string WebhookUrl { get; set; }
    }

    // PayOS Response Models
    public class PayOSCreateLinkResponse
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PayOSPaymentData Data { get; set; }
    }

    public class PayOSPaymentData
    {
        public string Bin { get; set; }
        public string CheckoutUrl { get; set; }
        public string QrCode { get; set; }
        public long OrderCode { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
        public List<PaymentItem> Items { get; set; }
    }

    public class PayOSPaymentInfo
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PayOSPaymentDetail Data { get; set; }
        public string Signature { get; set; }
    }

    public class PayOSPaymentDetail
    {
        public string Id { get; set; }
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public int AmountPaid { get; set; }
        public int AmountRemaining { get; set; }
        public string Status { get; set; } // PENDING, PAID, CANCELLED, EXPIRED
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string CancellationReason { get; set; }
        public List<PayOSTransaction> Transactions { get; set; }
    }
    
    public class PayOSTransaction
    {
        public int Amount { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string Reference { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string CounterAccountBankId { get; set; }
        public string CounterAccountBankName { get; set; }
        public string CounterAccountName { get; set; }
        public string CounterAccountNumber { get; set; }
        public string VirtualAccountName { get; set; }
        public string VirtualAccountNumber { get; set; }
    }

    public class PayOSWebhookData
    {
        public int Code { get; set; }
        public string Desc { get; set; }
        public PayOSWebhookInfo Data { get; set; }
        public string Signature { get; set; }
    }

    public class PayOSWebhookInfo
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string CounterAccountNumber { get; set; }
        public string Status { get; set; }
    }
}

