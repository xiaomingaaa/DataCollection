using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using Redis.Cache;
namespace MealCardCollection.Util
{
    /// <summary>
    /// redis缓存工具类----在此项目中暂时不启用缓存，等待优化吧
    /// </summary>
    class RedisCacheHelper
    {
        ItemCache<List<string>> item = new ItemCache<List<string>>();
        public void AddItem()
        {

        }
        List<string> info = new List<string>();
        
    }
}
