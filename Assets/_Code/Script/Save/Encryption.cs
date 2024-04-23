using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ivayami.Save {
    public static class Encryption {

        private const string key = "If_You_Decrypt_This_You_Gay89012";
        private const string iv = "But_Fine_I_Guess";

        public static string Encrypt(string text) {
            using (Aes aes = Aes.Create()) {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                using (StreamWriter streamWritter = new StreamWriter(cryptoStream)) {
                    streamWritter.Write(text);
                    streamWritter.Close();
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public static string Decrypt(string cipher) {
            using (Aes aes = Aes.Create()) {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipher)))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream)) return streamReader.ReadToEnd();
            }
        }

    }
}
