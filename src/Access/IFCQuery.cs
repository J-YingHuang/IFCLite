using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFCLite.Data;

namespace IFCLite.Access
{
    public class IFCQuery
    {
        private IFCDatabase IFCDatabase { get; set; }
        public IFCQuery(IFCDatabase db)
        {
            IFCDatabase = db;
        }
        public IFCObject GetObjectByP21Id(string p21id)
        {
            IFCReplaceRecord replace = IFCDatabase.ReplaceTable.FindOne(x => x.KeyElement == p21id);
            int count = IFCDatabase.IFCModel.Count();
            if (replace == null) //不是被取代的P21Id
                return new IFCObject(IFCDatabase.IFCModel.FindOne(x => x["P21Id"] == p21id));
            else //須找到取代的資料
            {
                IFCObject obj = new IFCObject(IFCDatabase.IFCModel.FindOne(x => x["P21Id"] == replace.ValueElement));
                obj.P21Id = p21id;
                return obj;
            }
        }
    }
}
