using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace B_M.Services
{
    public class CryptoProvider
    {
        /// <summary>
        /// Create signature for payment request using specific fields
        /// Format: amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
        /// </summary>
        public string CreatePaymentRequestSignature(Dictionary<string, object> data, string key)
        {
            if (data == null || string.IsNullOrEmpty(key))
                return null;

            // Convert to camelCase if needed
            data = ConvertToCamelCase(data);

            // Extract specific fields for payment request signature
            var requiredFields = new[] { "amount", "cancelUrl", "description", "orderCode", "returnUrl" };
            var values = new List<string>();

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field))
                    return null;

                values.Add($"{field}={ConvertValueToString(data[field])}");
            }

            var dataString = string.Join("&", values);

            // Create HMAC-SHA256 signature
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Create signature from object data (for webhook verification)
        /// Uses all fields sorted alphabetically
        /// </summary>
        public string CreateSignatureFromObject(Dictionary<string, object> data, string key)
        {
            if (data == null || string.IsNullOrEmpty(key))
                return null;

            // Convert to camelCase
            data = ConvertToCamelCase(data);

            // Sort by key
            var sortedData = data.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            // Convert to query string
            var queryString = ConvertObjectToQueryString(sortedData);

            // Create HMAC-SHA256 signature
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Convert object to query string format
        /// </summary>
        private string ConvertObjectToQueryString(Dictionary<string, object> obj)
        {
            var parts = new List<string>();

            foreach (var kvp in obj.OrderBy(x => x.Key))
            {
                var key = kvp.Key;
                var value = kvp.Value;

                // Skip null values
                if (value == null)
                    value = "";
                else if (value is List<object> || value is object[])
                {
                    // Serialize arrays as JSON
                    value = JsonConvert.SerializeObject(value);
                }
                else
                {
                    value = ConvertValueToString(value);
                }

                parts.Add($"{key}={value}");
            }

            return string.Join("&", parts);
        }

        /// <summary>
        /// Convert value to string with proper boolean handling
        /// </summary>
        private string ConvertValueToString(object value)
        {
            if (value == null)
                return "";
            
            if (value is bool)
                return ((bool)value) ? "true" : "false";
            
            return value.ToString();
        }

        /// <summary>
        /// Convert dictionary to camelCase keys
        /// </summary>
        private Dictionary<string, object> ConvertToCamelCase(Dictionary<string, object> data)
        {
            var result = new Dictionary<string, object>();
            
            foreach (var kvp in data)
            {
                var camelKey = ToCamelCase(kvp.Key);
                result[camelKey] = kvp.Value;
            }
            
            return result;
        }

        /// <summary>
        /// Convert string to camelCase
        /// </summary>
        private string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            if (str.Length == 1)
                return str.ToLower();
            
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Convert JObject to Dictionary for signature generation
        /// </summary>
        public Dictionary<string, object> ConvertJObjectToDictionary(JObject jObject)
        {
            var dict = new Dictionary<string, object>();
            
            foreach (var prop in jObject.Properties())
            {
                if (prop.Value.Type == JTokenType.Object || prop.Value.Type == JTokenType.Array)
                {
                    dict[prop.Name] = prop.Value.ToString();
                }
                else
                {
                    dict[prop.Name] = prop.Value.ToObject<object>();
                }
            }
            
            return dict;
        }
    }
}

