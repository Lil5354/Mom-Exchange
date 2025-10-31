using System;
using System.Text;

namespace B_M.Helpers
{
    public class QRCodeHelper
    {
        /// <summary>
        /// Generate QR code image URL from VietQR text using external API
        /// </summary>
        public static string GenerateQRCodeImageUrl(string vietQRText)
        {
            if (string.IsNullOrEmpty(vietQRText))
                return null;

            // Encode the VietQR text
            var encodedText = Uri.EscapeDataString(vietQRText);
            
            // Using QRServer API (free and reliable)
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={encodedText}";
            
            // Alternative: Using QuickChart API
            // return $"https://quickchart.io/qr?text={encodedText}&size=300";
        }

        /// <summary>
        /// Parse VietQR text to extract payment information
        /// VietQR format: 00020101021238520010A00000072701270006VietQR...01303806903100104048440405203042005702VN6210020962837458986304
        /// </summary>
        public static VietQRInfo ParseVietQRText(string vietQRText)
        {
            if (string.IsNullOrEmpty(vietQRText))
                return null;

            try
            {
                var info = new VietQRInfo
                {
                    AccountNumber = ExtractVietQRField(vietQRText, "01"),
                    AccountName = ExtractVietQRField(vietQRText, "02"),
                    BankBin = ExtractVietQRField(vietQRText, "38"),
                    Amount = ExtractVietQRField(vietQRText, "54"),
                    Content = ExtractVietQRField(vietQRText, "08")
                };

                return info;
            }
            catch
            {
                return null;
            }
        }

        private static string ExtractVietQRField(string vietQR, string tag)
        {
            // VietQR format: TagLengthData
            // Example: "0106123456" means tag 01, length 06, data "123456"
            var index = vietQR.IndexOf(tag);
            if (index < 0) return null;

            var lengthStr = vietQR.Substring(index + 2, 2);
            if (int.TryParse(lengthStr, out int length))
            {
                var data = vietQR.Substring(index + 4, length);
                return data;
            }

            return null;
        }
    }

    public class VietQRInfo
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankBin { get; set; }
        public string Amount { get; set; }
        public string Content { get; set; }
    }
}

