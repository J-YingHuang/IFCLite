using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using IFCLite.Data;

namespace IFCLite.Access
{
    public class IFCUpdate
    {
        private IFCDatabase Database { get; set; }
        public IFCUpdate(IFCDatabase Database) { this.Database = Database; }
        public bool Object(IFCObject ifcObject)
        {
            return Database.IFCModel.Update(ifcObject.ToBson());
        }
        public int Objects(List<IFCObject> ifcObjects)
        {
            return Database.IFCModel.Update(GetObjects(ifcObjects.ToList<IFCBase>()));
        }
        public bool FileDescription(IFCHeader header)
        {
            if (header.EntityName != "FILE_DESCRIPTION")
                return false;
            Database.IFCHead.Update(header.ToBson());
            return true;
        }
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
