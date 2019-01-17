using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealCardCollection.Entity
{
    class XfDataEntity
    {
        private string info;
        private int id;
        private string xftime;
        private int isupload;

        public string Info { get => info; set => info = value; }
        public int Id { get => id; set => id = value; }
        public string Xftime { get => xftime; set => xftime = value; }
        public int Isupload { get => isupload; set => isupload = value; }
        public XfDataEntity(string info,int id,string xftime,int upload)
        {
            Info = info;
            Id = id;
            Xftime = xftime;
            Isupload = upload;
        }
    }
}
