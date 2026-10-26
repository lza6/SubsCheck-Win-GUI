using System;
using System.Security.Cryptography;
using System.Text;

namespace subs_check.win.gui
{
    /// <summary>
    /// 基于 Windows DPAPI 的本地机密信息加密助手。
    /// 密文仅能在同一 Windows 用户账户下、同一台机器上解密，适合存放 GitHub Token、
    /// Worker Token、WebDAV 密码等本地凭据，避免明文落盘 config.yaml。
    /// </summary>
    internal static class CryptoHelper
    {
        /// <summary>密文前缀，用于识别已是 DPAPI 密文的字段，避免重复加密。</summary>
        private const string EncryptedPrefix = "dpapi:";

        /// <summary>
        /// 加密（附加熵使用固定盐，保证同一用户跨会话可解密）。
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] entropy = Encoding.UTF8.GetBytes("SubsCheck-Win-GUI/v1");
            byte[] cipherBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.CurrentUser);
            return EncryptedPrefix + Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 解密。已是密文则解密；明文（旧版配置）原样返回，供调用方迁移回写。
        /// </summary>
        public static string Decrypt(string storedValue)
        {
            if (string.IsNullOrEmpty(storedValue)) return storedValue;
            if (!IsEncrypted(storedValue)) return storedValue;

            string base64 = storedValue.Substring(EncryptedPrefix.Length);
            byte[] cipherBytes = Convert.FromBase64String(base64);
            byte[] entropy = Encoding.UTF8.GetBytes("SubsCheck-Win-GUI/v1");
            byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>是否为 DPAPI 密文。</summary>
        public static bool IsEncrypted(string storedValue)
        {
            return !string.IsNullOrEmpty(storedValue) && storedValue.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// 生成高熵随机密钥（WebUI API Key 用），去除易混淆字符。
        /// </summary>
        public static string GenerateApiKey()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            byte[] randomBytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            StringBuilder sb = new StringBuilder(32);
            foreach (byte b in randomBytes)
            {
                sb.Append(chars[b % chars.Length]);
            }
            return sb.ToString();
        }
    }
}
