using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace MedicalEquipmentManagement.Helpers
{
    /// <summary>Giữ định dạng hash PBKDF2-SHA256 của Beta và sử dụng Windows CNG để dẫn xuất khóa.</summary>
    public static class PasswordHasher
    {
        private const string Version = "pbkdf2-sha256-v1";
        private const ulong Iterations = 600000;

        /// <summary>Tạo salt ngẫu nhiên 16 byte và lưu khóa 32 byte với 600.000 vòng lặp theo định dạng cũ.</summary>
        /// <param name="password">Mật khẩu mới đã được xác thực theo chính sách độ dài.</param>
        /// <returns>Chuỗi gồm phiên bản, salt Base64 và khóa Base64.</returns>
        public static string Hash(string password)
        {
            var salt = new byte[16];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(salt);
            return Version + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(Derive(password, salt));
        }

        /// <summary>Kiểm tra định dạng và so toàn bộ byte khóa, giúp tránh lộ vị trí byte sai qua thời gian so sánh.</summary>
        /// <param name="password">Mật khẩu người dùng nhập.</param>
        /// <param name="storedHash">Hash lấy từ tài khoản trong database.</param>
        /// <returns>Kết quả xác minh. Hash lỗi định dạng hoặc sai phiên bản trả về false.</returns>
        public static bool Verify(string password, string storedHash)
        {
            var fields = storedHash?.Split('$');
            if (password == null || fields == null || fields.Length != 3 || fields[0] != Version)
                return false;

            try
            {
                var salt = Convert.FromBase64String(fields[1]);
                var expected = Convert.FromBase64String(fields[2]);
                if (salt.Length != 16 || expected.Length != 32)
                    return false;

                var actual = Derive(password, salt);
                int difference = 0;
                for (int i = 0; i < expected.Length; i++)
                    difference |= expected[i] ^ actual[i];
                return difference == 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        internal static string SessionCredential(string storedHash)
        {
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(storedHash)));
        }

        /// <summary>Mã hóa mật khẩu UTF-8 và gọi Windows CNG với cùng tham số của triển khai .NET Framework cũ.</summary>
        private static byte[] Derive(string password, byte[] salt)
        {
            const int hmacProviderFlag = 8;
            var secret = Encoding.UTF8.GetBytes(password);
            var key = new byte[32];
            try
            {
                int status = BCryptOpenAlgorithmProvider(out IntPtr provider, "SHA256", null, hmacProviderFlag);
                if (status != 0)
                    throw new CryptographicException(status);
                try
                {
                    status = BCryptDeriveKeyPBKDF2(provider, secret, secret.Length, salt, salt.Length,
                        Iterations, key, key.Length, 0);
                    if (status != 0)
                        throw new CryptographicException(status);
                    return key;
                }
                finally
                {
                    BCryptCloseAlgorithmProvider(provider, 0);
                }
            }
            finally
            {
                Array.Clear(secret, 0, secret.Length);
            }
        }

        [DllImport("bcrypt.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int BCryptOpenAlgorithmProvider(out IntPtr provider, string algorithm, string implementation, int flags);

        [DllImport("bcrypt.dll", ExactSpelling = true)]
        private static extern int BCryptDeriveKeyPBKDF2(IntPtr provider, byte[] password, int passwordLength,
            byte[] salt, int saltLength, ulong iterations, [Out] byte[] key, int keyLength, int flags);

        [DllImport("bcrypt.dll", ExactSpelling = true)]
        private static extern int BCryptCloseAlgorithmProvider(IntPtr provider, int flags);
    }
}
