using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCLite.Data
{
    public class IFCQuery
    {
        private LiteCollection<BsonDocument> IFCModel { get; set; }
        private LiteCollection<BsonDocument> IFCHead { get; set; }
        private LiteCollection<IFCReplaceRecord> ReplaceTable { get; set; }
        private LiteCollection<IFCInverseRecord> InverseTable { get; set; }

        public IFCQuery(LiteCollection<BsonDocument> IFCModel, LiteCollection<BsonDocument> IFCHead, LiteCollection<IFCReplaceRecord> ReplaceTable,
            LiteCollection<IFCInverseRecord> InverseTable)
        {
            this.IFCModel = IFCModel;
            this.IFCHead = IFCHead;
            this.ReplaceTable = ReplaceTable;
            this.InverseTable = InverseTable;
        }
        public IFCObject GetObjectByP21Id(string p21Id)
        {
            IFCReplaceRecord replace = ReplaceTable.FindOne(x => x.KeyElement == p21Id);
            if (replace == null) //不是被取代的P21Id
                return new IFCObject(IFCModel.FindOne(x => x["P21Id"] == p21Id));
            else //須找到取代的資料
            {
                IFCObject obj = new IFCObject(IFCModel.FindOne(x => x["P21Id"] == replace.ValueElement));
                obj.P21Id = p21Id;
                return obj;
            }
        }
    }
}
