using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using B_M.Models;

namespace B_M.Repositories
{
    /// <summary>
    /// Repository for managing application settings
    /// Handles CRUD operations with encryption support
    /// </summary>
    public class SettingsRepository : IDisposable
    {
        private ApplicationDbContext db;
        private static readonly string EncryptionKey = "MomExchange2025!@#$%SecureKey32"; // Should be in Web.config

        public SettingsRepository()
        {
            try
            {
                db = new ApplicationDbContext();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsRepository init error: {ex.Message}");
                // Will use fallback in get methods
            }
        }

        #region Get Settings

        /// <summary>
        /// Get string setting value
        /// </summary>
        public string GetSetting(string category, string key, string defaultValue = null)
        {
            try
            {
                // Safe: Return default if db not initialized or table not exists
                if (db == null)
                    return defaultValue;

                var setting = db.ApplicationSettings
                    .FirstOrDefault(s => s.Category == category && s.Key == key);

                if (setting == null)
                    return defaultValue;

                if (setting.IsEncrypted)
                    return Decrypt(setting.Value);

                return setting.Value ?? defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSetting Error ({category}.{key}): {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Get integer setting value
        /// </summary>
        public int GetSettingInt(string category, string key, int defaultValue = 0)
        {
            var value = GetSetting(category, key);
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Get boolean setting value
        /// </summary>
        public bool GetSettingBool(string category, string key, bool defaultValue = false)
        {
            var value = GetSetting(category, key);
            if (bool.TryParse(value, out bool result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Get DateTime setting value
        /// </summary>
        public DateTime? GetSettingDateTime(string category, string key, DateTime? defaultValue = null)
        {
            var value = GetSetting(category, key);
            if (DateTime.TryParse(value, out DateTime result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Get all settings for a category
        /// </summary>
        public Dictionary<string, string> GetCategorySettings(string category)
        {
            try
            {
                // Safe: Return empty if db not initialized
                if (db == null)
                    return new Dictionary<string, string>();

                var settings = db.ApplicationSettings
                    .Where(s => s.Category == category)
                    .ToDictionary(
                        s => s.Key,
                        s => s.IsEncrypted ? Decrypt(s.Value) : s.Value
                    );

                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCategorySettings Error ({category}): {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        #endregion

        #region Set Settings

        /// <summary>
        /// Set setting value (creates if not exists, updates if exists)
        /// </summary>
        public bool SetSetting(string category, string key, object value, string dataType = "String", bool encrypt = false, int? updatedBy = null)
        {
            try
            {
                // Safe: Return false if db not initialized
                if (db == null)
                    return false;

                var setting = db.ApplicationSettings
                    .FirstOrDefault(s => s.Category == category && s.Key == key);

                string stringValue = value?.ToString() ?? "";
                
                if (encrypt)
                    stringValue = Encrypt(stringValue);

                if (setting == null)
                {
                    // Create new setting
                    setting = new ApplicationSetting
                    {
                        Category = category,
                        Key = key,
                        Value = stringValue,
                        DataType = dataType,
                        IsEncrypted = encrypt,
                        LastUpdated = DateTime.Now,
                        UpdatedBy = updatedBy
                    };
                    db.ApplicationSettings.Add(setting);
                }
                else
                {
                    // Update existing setting
                    setting.Value = stringValue;
                    setting.DataType = dataType;
                    setting.IsEncrypted = encrypt;
                    setting.LastUpdated = DateTime.Now;
                    setting.UpdatedBy = updatedBy;
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetSetting Error ({category}.{key}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save multiple settings at once
        /// </summary>
        public bool SaveSettings(string category, Dictionary<string, object> settings, int? updatedBy = null)
        {
            try
            {
                // Safe: Return false if db not initialized
                if (db == null)
                {
                    System.Diagnostics.Debug.WriteLine("SaveSettings: Database not initialized (migration may not have run)");
                    return false;
                }

                foreach (var kvp in settings)
                {
                    // Determine if should encrypt (passwords, tokens, etc.)
                    bool shouldEncrypt = kvp.Key.ToLower().Contains("password") || 
                                        kvp.Key.ToLower().Contains("token") ||
                                        kvp.Key.ToLower().Contains("secret");

                    SetSetting(category, kvp.Key, kvp.Value, GetDataType(kvp.Value), shouldEncrypt, updatedBy);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveSettings Error ({category}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a setting
        /// </summary>
        public bool DeleteSetting(string category, string key)
        {
            try
            {
                var setting = db.ApplicationSettings
                    .FirstOrDefault(s => s.Category == category && s.Key == key);

                if (setting != null)
                {
                    db.ApplicationSettings.Remove(setting);
                    db.SaveChanges();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteSetting Error ({category}.{key}): {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get data type from object
        /// </summary>
        private string GetDataType(object value)
        {
            if (value == null) return "String";
            if (value is bool) return "Bool";
            if (value is int) return "Int";
            if (value is DateTime) return "DateTime";
            return "String";
        }

        /// <summary>
        /// Encrypt sensitive data (simple AES encryption)
        /// </summary>
        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 32));
                byte[] iv = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 16));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var msEncrypt = new System.IO.MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Encryption Error: {ex.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// Decrypt sensitive data
        /// </summary>
        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 32));
                byte[] iv = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 16));
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var msDecrypt = new System.IO.MemoryStream(buffer))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption Error: {ex.Message}");
                return cipherText;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            db?.Dispose();
        }

        #endregion
    }
}

