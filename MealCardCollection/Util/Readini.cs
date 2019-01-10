using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
namespace MealCardCollection.Util
{
    /// <summary>
    /// 读取配置文件
    /// </summary>
    public class Readini
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        
        public static long WriteIniFile(string section,string key,string value,string filepath)
        {
            long flag = -1;
            if (FileIsExist(filepath))
            {
                flag= WritePrivateProfileString(section, key, value, filepath);
            }
            return flag;
        }
        public static string ReadIniFile(string section,string key,string filepath)
        {
            StringBuilder temp = new StringBuilder(500);
            if (FileIsExist(filepath))
            {                
                int i = GetPrivateProfileString(section, key, "", temp, 500, filepath);                
            }
            return temp.ToString().Trim();
        }
        private static bool FileIsExist(string path)
        {
            return File.Exists(path);
        }
    }
}
