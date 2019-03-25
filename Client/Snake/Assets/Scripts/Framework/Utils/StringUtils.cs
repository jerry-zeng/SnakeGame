using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Framework
{
    //TODO: these functions were not implemented yet.
    public static class StringUtils 
    {
        public static byte[] Decompress(byte[] data)
        {
            return data;
        }

        public static string DecompressToString(byte[] data)
        {
            data = Decompress(data);
            string ret = Encoding.UTF8.GetString( data );
            return ret;
        }

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

        public static string ToMD5(byte[] data)
        {
            string ret = Encoding.UTF8.GetString( data );
            return ret;
        }
    }
}
