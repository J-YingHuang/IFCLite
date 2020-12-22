using LiteDB;

namespace IFCLite.Data
{
    public class IFCDatabase
    {
        public string Path { get; }
        private LiteDatabase db { get; set; }
        public LiteCollection<BsonDocument> IFCModel { get { return db.GetCollection("IFCModel") as LiteCollection<BsonDocument>; } }
        public LiteCollection<BsonDocument> IFCHead { get { return db.GetCollection("IFCHead") as LiteCollection<BsonDocument>; } }
        public LiteCollection<IFCReplaceRecord> ReplaceTable { get { return db.GetCollection<IFCReplaceRecord>() as LiteCollection<IFCReplaceRecord>; } }
        public LiteCollection<IFCInverseRecord> InverseTable { get { return db.GetCollection<IFCInverseRecord>() as LiteCollection<IFCInverseRecord>; } }
        public IFCDatabase(string dbPath, bool create)
        {
            Path = dbPath;
            db = new LiteDatabase(dbPath);
            if(create)
                CreateIndex();
        }
        private void CreateIndex()
        {
            IFCModel.EnsureIndex(x => x["P21Id"]);
            IFCModel.EnsureIndex(x => x["EntityName"]);
            IFCModel.EnsureIndex(x => x["GlobalId"]);
            ReplaceTable.EnsureIndex(x => x.KeyElement);
            InverseTable.EnsureIndex(x => x.KeyElement);
        }
        public void Close()
        {
            db.Dispose();
        }

    }
}
