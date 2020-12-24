using AutoMapper;
using IFCLite.Data;
using LiteDB;
using System.Collections.Generic;

namespace IFCLite.Access
{
    public class IFCQuery
    {
        private IFCDatabase Database { get; set; }
        public IFCQuery(IFCDatabase db)
        {
            Database = db;
        }

        //IFCModel
        /// <summary>
        /// 透過P21Id找IFC物件。
        /// </summary>
        /// <param name="p21Id">P21Id</param>
        /// <returns></returns>
        public IFCObject ByP21Id(string p21Id)
        {
            IFCObject res = new IFCObject(Database.IFCModel.FindOne(x => x["P21Id"] == p21Id));
            return res ?? GetRecoverObject(p21Id);
        }
        /// <summary>
        /// 透過GlobalId找IFC物件。
        /// </summary>
        /// <param name="globalId">GlobalId</param>
        /// <returns></returns>
        public IFCObject ByGlobalId(string globalId)
        {
            return new IFCObject(Database.IFCModel.FindOne(x => x["GlobalId"] == globalId));
        }
        /// <summary>
        /// 透過EntityName找IFC物件。
        /// </summary>
        /// <param name="entityName">EntityName</param>
        /// <returns></returns>
        public IEnumerable<IFCObject> ByEntityName(string entityName)
        {
            List<IFCObject> res = new List<IFCObject>();
            foreach (BsonDocument data in Database.IFCModel.Find(x => x["EntityName"] == entityName))
            {
                IFCObject obj = new IFCObject(data);
                yield return obj;
                foreach (IFCObject recover in GetAllRecoverObjectByReplace(obj))
                    yield return recover;//被取代的物件需要還原
            }
        }

        //IFCHead
        public IFCHeader GetFileDescription()
        {
            return new IFCHeader(Database.IFCHead.FindOne(x => x["EntityName"] == "FILE_DESCRIPTION"));
        }
        public IFCHeader GetFileName()
        {
            return new IFCHeader(Database.IFCHead.FindOne(x => x["EntityName"] == "FILE_NAME"));
        }
        public IFCHeader GetFileSchema()
        {
            return new IFCHeader(Database.IFCHead.FindOne(x => x["EntityName"] == "FILE_SCHEMA"));
        }

        /// <summary>
        /// 取得被取代的IFC物件，若該物件未被取代則回傳NULL。
        /// </summary>
        /// <param name="p21Id">被取代的P21Id</param>
        /// <returns></returns>
        private IFCObject GetRecoverObject(string p21Id)
        {
            IFCReplaceRecord replace = Database.ReplaceTable.FindOne(x => x.KeyElement == p21Id);
            return new IFCObject(Database.IFCModel.FindOne(x => x["P21Id"] == replace.ValueElement)) { P21Id = p21Id };
        }
        /// <summary>
        /// 透過取代者，還原所有被取代的IFC物件。
        /// </summary>
        /// <param name="oldObject">取代者物件</param>
        /// <returns></returns>
        private IEnumerable<IFCObject> GetAllRecoverObjectByReplace(IFCObject oldObject)
        {
            List<string> replaceData = new List<string>();
            foreach (IFCReplaceRecord record in Database.ReplaceTable.Find(x => x.ValueElement == oldObject.P21Id))
                replaceData.Add(record.KeyElement);
            List<IFCObject> res = new List<IFCObject>();
            foreach (string repId in replaceData)
            {
                IFCObject newObj = CopyObject(oldObject);
                newObj.P21Id = repId;
                yield return newObj;
            }
        }

        /// <summary>
        /// 深複製IFC物件。
        /// </summary>
        /// <param name="data">需複製的IFC物件</param>
        /// <returns></returns>
        private IFCObject CopyObject(IFCObject data)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<IFCObject, IFCObject>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper.Map<IFCObject>(data);
        }
    }
}
