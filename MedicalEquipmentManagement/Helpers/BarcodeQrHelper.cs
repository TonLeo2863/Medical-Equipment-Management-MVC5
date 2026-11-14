using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using QRCoder;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>
    /// Helper tự động sinh mã QR Code và Barcode (Code 128) từ Số Serial / Mã tài sản của thiết bị y tế.
    /// Phục vụ yêu cầu mở rộng của Module 2: Tự động sinh mã vạch (Barcode/QR Code) dựa trên Số Serial.
    /// </summary>
    public static class BarcodeQrHelper
    {
        /// <summary>
        /// Sinh ảnh PNG QR Code chất lượng cao từ nội dung văn bản (sử dụng thư viện QRCoder).
        /// Không cần lưu file tạm ra ổ cứng, trả thẳng mảng byte PNG để stream về trình duyệt.
        /// </summary>
        /// <param name="payload">Chuỗi dữ liệu mã hóa (Số Serial hoặc chuỗi thông tin tài sản)</param>
        /// <param name="pixelsPerModule">Kích thước mỗi điểm ảnh QR (mặc định 10)</param>
        public static byte[] GenerateQrCodePng(string payload, int pixelsPerModule = 10)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                payload = "N/A";
            }

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(pixelsPerModule);
            }
        }

        /// <summary>
        /// Sinh ảnh PNG Barcode chuẩn Code 128-B từ Số Serial (vẽ trực tiếp bằng System.Drawing).
        /// Giúp dán nhãn mã vạch 1D tiêu chuẩn lên thân máy y tế để đầu đọc tia laser quét trực tiếp.
        /// </summary>
        public static byte[] GenerateCode128BarcodePng(string serialNumber, string assetCodeLabel = null, int barHeight = 70)
        {
            var text = string.IsNullOrWhiteSpace(serialNumber) ? "UNKNOWN" : serialNumber.Trim();

            // Mã hóa chuỗi ASCII sang chuỗi bit đen/trắng chuẩn Code 128-B
            var modules = EncodeCode128B(text);
            int quietZone = 12;
            int barScale = 2; // mỗi module rộng 2px để máy quét đọc rõ
            int width = Math.Max((modules.Length + quietZone * 2) * barScale, 240);
            int height = barHeight + 36;

            using (var bmp = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                int startX = (width - modules.Length * barScale) / 2;
                using (var blackBrush = new SolidBrush(Color.Black))
                {
                    for (int i = 0; i < modules.Length; i++)
                    {
                        if (modules[i] == '1')
                        {
                            g.FillRectangle(blackBrush, startX + i * barScale, 8, barScale, barHeight - 8);
                        }
                    }

                    // Vẽ dòng chữ Số Serial bên dưới mã vạch
                    var caption = string.IsNullOrWhiteSpace(assetCodeLabel)
                        ? "S/N: " + text
                        : assetCodeLabel + "  |  S/N: " + text;

                    using (var font = new Font("Consolas", 10f, FontStyle.Bold, GraphicsUnit.Point))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center };
                        g.DrawString(caption, font, blackBrush, new RectangleF(0, barHeight + 4, width, 28), sf);
                    }
                }

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        // Bảng mẫu vạch chuẩn ISO/IEC 15417 cho Code 128 (Values 0..106)
        private static readonly string[] Code128Patterns = new[]
        {
            "11011001100","11001101100","11001100110","10010011000","10010001100","10001001100","10011001000","10011000100",
            "10001100100","11001001000","11001000100","11000100100","10110011100","10011011100","10011001110","10111001100",
            "10011101100","10011100110","11001110010","11001011100","11001001110","11011100100","11001110100","11101101110",
            "11101001100","11100101100","11100100110","11101100100","11100110100","11100110010","11011011000","11011000110",
            "11000110110","10100011000","10001011000","10001000110","10110001000","10001101000","10001100010","11010001000",
            "11000101000","11000100010","10110111000","10110001110","10001101110","10111011000","10111000110","10001110110",
            "11101110110","11010001110","11000101110","11011101000","11011100010","11011101110","11101011000","11101000110",
            "11100010110","11101101000","11101100010","11100011010","11101111010","11001000010","11110001010","10100110000",
            "10100001100","10010110000","10010000110","10000101100","10000100110","10110010000","10110000100","10011010000",
            "10011000010","10000110100","10000110010","11000010010","11001010000","11110111010","11000010100","10001111010",
            "10100111100","10010111100","10010011110","10111100100","10011110100","10011110010","11110100100","11110010100",
            "11110010010","11011011110","11011110110","11110110110","10101111000","10100011110","10001011110","10111101000",
            "10111100010","11110101000","11110100010","10111011110","10111101110","11101011110","11110101110","11010000100",
            "11010010000","11010011100","1100011101011"
        };

        private static string EncodeCode128B(string input)
        {
            const int startCodeB = 104;
            const int stopCode = 106;

            var sb = new StringBuilder();
            sb.Append(Code128Patterns[startCodeB]);
            int checksum = startCodeB;
            int weight = 1;

            foreach (char ch in input)
            {
                // Chỉ nhận các ký tự ASCII in được (32..126)
                int ascii = (ch >= 32 && ch <= 126) ? ch : '?';
                int codeValue = ascii - 32;
                sb.Append(Code128Patterns[codeValue]);
                checksum += codeValue * weight;
                weight++;
            }

            int checkIndex = checksum % 103;
            sb.Append(Code128Patterns[checkIndex]);
            sb.Append(Code128Patterns[stopCode]);
            return sb.ToString();
        }
    }
}
