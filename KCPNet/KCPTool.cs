/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    KPCNet/KCPTool 
* 功 能：       协议传输工具类（序列化、压缩）
* 类 名：       KCPTool
* 创建时间：  2024/8/8 19:12:01
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using SULog;
using SUNet;
using System.IO.Compression;
using Newtonsoft.Json;


    public static class KCPTool
    {
        private static readonly DateTime utcStart = new DateTime(1970, 1, 1);

        public static byte[] Serialize<T>(T msg) where T : KCPMsg
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, msg);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    SULogger.LogError($"Failed to serialize. Reson:{ex.Message}");
                    throw;
                }
            }
        }
        public static T Deserialize<T>(byte[] bytes) where T : KCPMsg
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                try
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    T msg =(T)binaryFormatter.Deserialize(memoryStream);
                    return msg;
                }
                catch (SerializationException ex)
                {
                    SULogger.LogError($"Failed to deSerialize. Reson:{ex.Message}");
                    throw;
                }
            }
        }

        
        public static byte[] Compress(byte[] input)
        {
            using(MemoryStream outMS = new MemoryStream())
            {
                using(GZipStream gzs = new GZipStream(outMS,CompressionMode.Compress,true))
                {
                    gzs.Write(input, 0, input.Length);
                    gzs.Close();
                    return outMS.ToArray();
                }
            }
        }
        public static byte[] DeCompress(byte[] input)
        {
            using(MemoryStream inputMS=new MemoryStream(input))
            {
                using(MemoryStream outMS = new MemoryStream())
                {
                    using(GZipStream gzs = new GZipStream(inputMS, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while((len = gzs.Read(bytes,0,bytes.Length))>0)
                        {
                            outMS.Write(bytes,0,len);
                        }
                        gzs.Close();
                        return outMS.ToArray() ;
                    }
                }

            }
        }

        public static ulong GetUTCStartMilliseconds()
        {
            return (ulong)(DateTime.UtcNow - utcStart).TotalMilliseconds;
        }

    }



