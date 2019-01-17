using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace MealCardCollection.Util
{
    /// <summary>
    /// 保存日志
    /// </summary>
    class Log
    {
        public static void WriteError(string str)
        {
            string log_path = AppDomain.CurrentDomain.BaseDirectory + "Logs" + "/error_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                str = DateTime.Now + "\r\n" + str;
                byte[] bytes = Encoding.Default.GetBytes(str + "\r\n");
                FileStream fileStream = File.OpenWrite(log_path);
                fileStream.Position = fileStream.Length;
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                fileStream.Close();
            }
            catch
            {

            }
        }
        public static void WriteLog(string str)
        {
            string log_path = AppDomain.CurrentDomain.BaseDirectory + "Logs" + "/log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                str = DateTime.Now + "\r\n" + str;
                byte[] bytes = Encoding.Default.GetBytes(str + "\r\n");
                FileStream fileStream = File.OpenWrite(log_path);
                fileStream.Position = fileStream.Length;
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                fileStream.Close();
            }
            catch
            {

            }
        }
        public static void WriteIgnoreData(string str)
        {
            string log_path = AppDomain.CurrentDomain.BaseDirectory + "Logs" + "/IgnoreXfData_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                str = DateTime.Now + "\r\n" + str;
                byte[] bytes = Encoding.Default.GetBytes(str + "\r\n");
                FileStream fileStream = File.OpenWrite(log_path);
                fileStream.Position = fileStream.Length;
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                fileStream.Close();
            }
            catch
            {

            }
        }
    }
}
