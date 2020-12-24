using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using IFCLite.Data;

namespace IFCLite.Access
{
    public class IFCDelete
    {
        private IFCDatabase Database { get; set; }
        public IFCDelete(IFCDatabase Database) { this.Database = Database; }
        /// <summary>
        /// 透過P21Id刪除物件。
        /// </summary>
        /// <param name="p21Id"></param>
        public void ByP21Id(string p21Id)
        {
            BsonDocument obj = Database.IFCModel.FindOne(x => x["P21Id"] == p21Id);
            if (obj == null) //屬於被取代的資料, 可直接刪除Replace資料即可
            {
                Database.ReplaceTable.DeleteMany(x => x.KeyElement == p21Id);
                return;
            }
            List<IFCReplaceRecord> record = Database.ReplaceTable.Find(x => x.ValueElement == p21Id).ToList(); //取得取代資料
            if(record.Count() != 0) //有取代別人, 需額外處理
            {
                string newReplaceId = record.First().KeyElement;
                record.Remove(record[0]); //刪除第一筆資料
                foreach (IFCReplaceRecord data in record)
                {
                    data.ValueElement = newReplaceId;
                    Database.ReplaceTable.Update(data); //更新ReplaceTable
                }
                obj["P21Id"] = newReplaceId; //更新P21Id
                Database.IFCModel.Insert(obj); //新增至資料庫
            }
            Database.IFCModel.Delete(obj["_id"]); //刪除該物件
        }
        /// <summary>
        /// 透過IFC物件刪除單一物件。
        /// </summary>
        /// <param name="ifcObject"></param>
        public void ByObject(IFCObject ifcObject)
        {
            ByP21Id(ifcObject.P21Id);
        }
        /// <summary>
        /// 透過IFC物件陣列刪除多個IFC物件。
        /// </summary>
        /// <param name="ifcObjects"></param>
        public void ByObject(List<IFCObject> ifcObjects)
        {
            foreach (IFCObject obj in ifcObjects)
                ByP21Id(obj.P21Id);
        }
    }
}
