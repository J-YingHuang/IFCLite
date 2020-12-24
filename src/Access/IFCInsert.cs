using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFCLite.Data;
using LiteDB;

namespace IFCLite.Access
{
    public class IFCInsert
    {
        private IFCDatabase Database { get; set; }
        public IFCInsert(IFCDatabase Database) { this.Database = Database; }
        /// <summary>
        /// 新增IFC物件至資料庫中。
        /// </summary>
        /// <param name="obj">IFC物件</param>
        public void IFCObject(IFCObject obj)
        {
            Database.IFCModel.Insert(obj.ToBson());
        }
        /// <summary>
        /// 新增複數IFC物件至資料庫中。
        /// </summary>
        /// <param name="objs">IFC物件</param>
        public void IFCObject(List<IFCObject> objs)
        {
            Database.IFCModel.Insert(GetObjects(objs.ToList<IFCBase>()));
        }
        internal void IFCHeadInsert(List<IFCHeader> header)
        {
            Database.IFCHead.Insert(GetObjects(header.ToList<IFCBase>()));
        }
        private IEnumerable<BsonDocument> GetObjects(List<IFCBase> objs)
        {
            foreach (IFCBase data in objs)
                yield return data.ToBson();//為了提升資料加入資料庫之速度所以才這樣處理
        }

    }
}
