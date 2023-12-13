using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Google.Protobuf;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.MessageWrapper
{
    public static class MessageWrapper
    {
#if PACKET_TYPE_INT
        public readonly static Dictionary<Type, uint> PacketMap = new Dictionary<Type, uint>()
        {
            // example
            //{ typeof(Vector3), (uint)PacketType.Pvector3 },
        };
#else
        public readonly static Dictionary<Type, ushort> PacketMap = new Dictionary<Type, ushort>()
        {
            // example
            //{ typeof(Vector3), (ushort)PacketType.Pvector3 },
        };
#endif

        /// <summary>
        /// Serialize the message with PacketWrapper using little-endian [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            try
            {

#if PACKET_TYPE_INT
                // [packetSize (except this header)(2)][messageType(4)][message]
                uint messageType = PacketMap[typeof(T)];
#else
                // [packetSize (except this header)(2)][messageType(2)][message]
                ushort messageType = PacketMap[typeof(T)];
#endif
                int messageSize = message.CalculateSize();
                ushort packetSize = (ushort)(Defines.PACKET_DATATYPE_SIZE + messageSize);
                int offset = 0;

                ArraySegment<byte> buffer = new byte[Defines.PACKET_HEADER_SIZE + packetSize];
                packetSize.FromUInt16(buffer, ref offset);
#if PACKET_TYPE_INT
                // contains offset += sizeof(uint)
                messageType.FromUInt32(buffer, ref offset); // 4 bytes
#else
                // contains offset += sizeof(ushort)
                messageType.FromUInt16(buffer, ref offset); // 2 bytes
#endif
                message.WriteTo(buffer.Slice(offset));

                // needless code
                // offset += packetSize - sizeof(ushort) or sizeof(uint);

                return buffer;
            }
            catch (KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", knfe, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch (Exception e)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", e, "Other Exception");
                return null;
            }
        }

        /// <summary>
        /// Extended function of PacketWrapper.Serialize().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> SerializeWrapper<T>(this T message) where T : IMessage
        {
            return Serialize(message);
        }



        // TODO : temp
        public static byte[] AES_KEY = Convert.FromBase64String("Irv3uhgf9WqnsxbU9KTTQGd3sHSL9ZbDeRgsxYj4jsY=");
        public static byte[] AES_IV = Convert.FromBase64String("ahvQXaMrxFfhtl+AJn0L0g==");

        /// <summary>
        /// Encrypt andSerialize the message with PacketWrapper using little-endian [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> SerializeEncrypt<T>(T message) where T : IMessage
        {
            try
            {

#if PACKET_TYPE_INT
                // [packetSize (except this header)(2)][messageType(4)][message]
                uint messageType = PacketMap[typeof(T)];
#else
                // [packetSize (except this header)(2)][messageType(2)][message]
                ushort messageType = PacketMap[typeof(T)];
#endif

                // header 제외하고 암호화 (타입 + 메시지 암호화)
                int messageSize = message.CalculateSize();

                // 메시지 타입과 메시지를 담고 있는 임시 버퍼 생성
                byte[] temp = new byte[Defines.PACKET_DATATYPE_SIZE + messageSize];

#if PACKET_TYPE_INT
                messageType.FromUInt32(temp, offset: 0); // 4bytes
#else
                messageType.FromUInt16(temp, offset: 0); // 2bytes
#endif
                message.WriteTo(new ArraySegment<byte>(temp, Defines.PACKET_DATATYPE_SIZE, messageSize));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = AES_KEY;
                    aes.IV = AES_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(AES_KEY, AES_IV))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        memoryStream.Position += Defines.PACKET_HEADER_SIZE;
                        // CryptoStream을 사용하여 암호화된 데이터를 MemoryStream에 쓰기
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(temp, 0, temp.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        var sendBuf = memoryStream.ToArray();
                        // write size at header(2)
                        ((ushort)(sendBuf.Length - Defines.PACKET_HEADER_SIZE)).FromUInt16(sendBuf, offset: 0);

                        return sendBuf;
                    }
                }
            }
            catch (KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.SerializeEncrypt", knfe, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch (CryptographicException ce)
            {
                CoreLogger.LogError("MessageWrapper.SerializeEncrypt", ce, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch (NotSupportedException nse)
            {
                CoreLogger.LogError("MessageWrapper.SerializeEncrypt", nse, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch (Exception e)
            {
                CoreLogger.LogError("MessageWrapper.SerializeEncrypt", e, "Other Exception");
                return null;
            }
        }

        /// <summary>
        /// Extended function of PacketWrapper.SerializeEncrypt().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> SerializeWrapperEncrypt<T>(this T message) where T : IMessage
        {
            return SerializeEncrypt(message);
        }

        public static byte[] Decrypt(ReadOnlySpan<byte> encryptedBytes)
        {
            // TODO : can remove copy?
            return Decrypt(encryptedBytes.ToArray());
        }

        public static byte[] Decrypt(byte[] encryptedBytes)
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