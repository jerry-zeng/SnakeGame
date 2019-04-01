using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Exception = System.Exception;

namespace Framework
{
    //TODO: these functions were not implemented yet.
    public static class StringUtils 
    {
        /// <summary>
        /// Convert byte[] to string with UTF8 encoding.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="data">Data.</param>
        public static string ToString(byte[] data)
        {
            string ret = Encoding.UTF8.GetString( data );
            return ret;
        }



        public static string StringToMD5(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            return BytesToMD5(Encoding.UTF8.GetBytes(str));
        }

        public static string FileToMd5(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";

            FileStream file = new FileStream(filePath, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            return BytesToMD5(retVal);
        }

        public static string BytesToMD5(byte[] data)
        {
            MD5 md5 = MD5.Create();
            data = md5.ComputeHash(data);

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; ++i)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }



        public static string DecompressToString(byte[] data)
        {
            data = Decompress(data);
            string ret = Encoding.UTF8.GetString( data );
            return ret;
        }


        /// <summary>
        /// Compress
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            try
            {
                var ms = new MemoryStream();
                var zip = new GZipStream(ms, CompressionMode.Compress, true);

                zip.Write(data, 0, data.Length);
                zip.Close();
                var buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Close();
                return buffer;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Decompress
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            try
            {
                var ms = new MemoryStream(data,2,data.Length-2);
                var zip = new DeflateStream(ms, CompressionMode.Decompress, true);
                var msreader = new MemoryStream();
                var buffer = new byte[100];
                while (true)
                {
                    int reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
