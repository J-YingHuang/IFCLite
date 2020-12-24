using LiteDB;

namespace IFCLite.Data
{
    public class IFCDatabase
    {
        public string Path { get; }
        private LiteDatabase DB { get; set; }
        public LiteCollection<BsonDocument> IFCModel { get { return DB.GetCollection("IFCModel") as LiteCollection<BsonDocument>; } }
        public LiteCollection<BsonDocument> IFCHead { get { return DB.GetCollection("IFCHead") as LiteCollection<BsonDocument>; } }
        public LiteCollection<IFCReplaceRecord> ReplaceTable { get { return DB.GetCollection<IFCReplaceRecord>() as LiteCollection<IFCReplaceRecord>; } }
        public LiteCollection<IFCInverseRecord> InverseTable { get { return DB.GetCollection<IFCInverseRecord>() as LiteCollection<IFCInverseRecord>; } }
        public IFCDatabase(string dbPath, bool create)
        {
            Path = dbPath;
            DB = new LiteDatabase(dbPath);
            if(create)
                CreateIndex();
        }
        private void CreateIndex()
        {
            IFCModel.EnsureIndex(x => x["P21Id"]);
            IFCModel.EnsureIndex(x => x["EntityName"]);
            IFCModel.EnsureIndex(x => x["GlobalId"]);
            ReplaceTable.EnsureIndex(x => x.KeyElement);
            ReplaceTable.EnsureIndex(x => x.ValueElement);
            InverseTable.EnsureIndex(x => x.KeyElement);
        }
        public void Close()
        {
            DB.Dispose();
        }

    }
}
