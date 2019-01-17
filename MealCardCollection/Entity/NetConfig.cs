using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealCardCollection.Entity
{
    class NetConfig
    {
        private string ipaddr;
        private int macid;

        public string Ipaddr { get => ipaddr; set => ipaddr = value; }
        public int Macid { get => macid; set => macid = value; }
        public NetConfig(string ipaddr, int macid)
        {
            Ipaddr = ipaddr;
            Macid = macid;
        }
    }
}
