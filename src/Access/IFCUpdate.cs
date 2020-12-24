using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using IFCLite.Data;

namespace IFCLite.Access
{
    /// <summary>
    /// 處理IFCDatabase更新事項
    /// </summary>
    public class IFCUpdate
    {
        private IFCDatabase Database { get; set; }
        /// <summary>
        /// 新建一個Update物件
        /// </summary>
        /// <param name="Database"></param>
        public IFCUpdate(IFCDatabase Database) { this.Database = Database; }
        /// <summary>
        /// 更新單一IFC物件
        /// </summary>
        /// <param name="ifcObject"></param>
        /// <returns></returns>
        public bool Object(IFCObject ifcObject)
        {
            return Database.IFCModel.Update(ifcObject.ToBson());
        }
        /// <summary>
        /// 更新多個IFC物件
        /// </summary>
        /// <param name="ifcObjects"></param>
        /// <returns></returns>
        public int Objects(List<IFCObject> ifcObjects)
        {
            return Database.IFCModel.Update(GetObjects(ifcObjects.ToList<IFCBase>()));
        }
        /// <summary>
        /// 更新IFC檔案描述
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public bool FileDescription(IFCHeader header)
        {
            if (header.EntityName != "FILE_DESCRIPTION")
                return false;
            Database.IFCHead.Update(header.ToBson());
            return true;
        }
        /// <summary>
        /// 更新IFC檔案名稱及相關內容
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public bool FileName(IFCHeader header)
        {
            if (header.EntityName != "FILE_NAME")
                return false;
            Database.IFCHead.Update(header.ToBson());
            return true;
        }
        private IEnumerable<BsonDocument> GetObjects(List<IFCBase> objs)
        {
            foreach (IFCBase data in objs)
                yield return data.ToBson();//為了提升資料加入資料庫之速度所以才這樣處理
        }

    }
}
