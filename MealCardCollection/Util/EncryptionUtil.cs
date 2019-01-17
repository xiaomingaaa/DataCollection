using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace MealCardCollection.Util
{
    class EncryptionUtil
    {
        /// <summary>
        /// 使用MD5加密明文，得到32位的密文字符串
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Md5Encryption(string message)
        {
            //声明MD5的加密容器
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.Default.GetBytes(message);
            byte[] encryBytes = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte t in encryBytes)
            {
                sb.Append(t.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
        /// <summary>
        /// 将明文转化成base64字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetBase64Encode(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string encriedStr = Convert.ToBase64String(bytes);
            return encriedStr;
        }
        /// <summary>
        /// 将经过base64加密过的字符串转换成明文字符串
        /// </summary>
        /// <param name="ciphertext">密文</param>
        /// <returns></returns>
        public static string GetBase64Decode(string ciphertext)
        {
            byte[] bytes = Convert.FromBase64String(ciphertext);
            string plaintext = Encoding.UTF8.GetString(bytes);
            return plaintext;
        }
    }
}
