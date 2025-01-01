using System.Security.Cryptography;
using System.Text;

namespace LCSC.Core.Helpers
{
    public static class CryptographyHelper
    {
        public static string Encrypt(string plainText, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using var aes = Aes.Create();
            aes.Key = keyBytes; aes.GenerateIV();
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            var iv = aes.IV;
            var encrypted = ms.ToArray();
            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedText, string key)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);
            var iv = new byte[16]; var cipher = new byte[16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
