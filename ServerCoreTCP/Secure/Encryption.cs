using ServerCoreTCP.CLogger;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ServerCoreTCP.Secure
{
    public class Encryption
    {
        public static bool UseAES { get; set; } = false;

        // TODO : temp
        public static byte[] AES_KEY = Convert.FromBase64String("Irv3uhgf9WqnsxbU9KTTQGd3sHSL9ZbDeRgsxYj4jsY=");
        public static byte[] AES_IV = Convert.FromBase64String("ahvQXaMrxFfhtl+AJn0L0g==");

        public static byte[] KEY = Convert.FromBase64String("Irv3uhgf9WqnsxbU9KTTQGd3sHSL9ZbDeRgsxYj4jsY=");


        public static byte[] Encrypt(byte[] buffer, int leftPaddingSize = Defines.PACKET_HEADER_SIZE)
        {
            if (UseAES) return EncryptAES(buffer, leftPaddingSize);
            else return EncryptXOR(buffer, leftPaddingSize);
        }

        public static byte[] Decrypt(ReadOnlySpan<byte> encryptedBytes)
        {
            // TODO : can remove copy?
            if (UseAES) return DecryptAES(encryptedBytes.ToArray());
            else return DecryptXOR(encryptedBytes.ToArray());
        }


        internal static byte[] EncryptXOR(byte[] data, int leftPaddingSize)
        {
            byte[] encrypted = new byte[leftPaddingSize + data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i + leftPaddingSize] = (byte)(data[i] ^ KEY[i % KEY.Length]);
            }
            return encrypted;
        }

        internal static byte[] DecryptXOR(byte[] data)
        {
            return EncryptXOR(data, 0); // XOR 연산은 암호화와 복호화가 같은 작업
        }

        internal static byte[] EncryptAES(byte[] buffer, int leftPaddingSize)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = AES_IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(AES_KEY, AES_IV))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Position += leftPaddingSize;
                    // CryptoStream을 사용하여 암호화된 데이터를 MemoryStream에 쓰기
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(buffer, 0, buffer.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    var res = memoryStream.ToArray();

                    return res;
                }
            }
        }

        public static byte[] DecryptAES(byte[] encryptedBytes)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = AES_KEY;
                    aes.IV = AES_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] decrypted;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(AES_KEY, AES_IV))
                    {
                        decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                        return decrypted;
                    }
                }
            }
            catch (Exception e)
            {
                CoreLogger.LogError("MessageWrapper.Decrypt", e, "Exception");
                return null;
            }

        }
    }
}
