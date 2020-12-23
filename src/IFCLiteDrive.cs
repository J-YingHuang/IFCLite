﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using IFCLite.Data.Schema;
using IFCLite.Data;
using IFCLite.IO;
using IFCLite.Access;

namespace IFCLite
{
    public class IFCLiteDrive
    {
        private IFCDatabase IFCDatabase { get; set; }
        private SchemaReader SchemaReader { get; set; }
        public IFCQuery Query { get { return new IFCQuery(IFCDatabase); } }
        
        public IFCLiteDrive(string dbPath, string ifcPath, IFCVersion version, bool create)
        {
            SchemaReader = new SchemaReader(version);
            IFCDatabase = new IFCDatabase(dbPath,create);
            AddDataToDatabase(ifcPath);
        }
        public IFCLiteDrive(string dbPath, IFCVersion version, bool create)
        {
            SchemaReader = new SchemaReader(version);
            IFCDatabase = new IFCDatabase(dbPath, create);
        }
        private void AddDataToDatabase(string ifcpath)
        {
            IFCReader reader = new IFCReader(ifcpath, SchemaReader);

            IFCDatabase.IFCModel.Insert(GetObjects(reader.InsertObjs.ToList<IFCBase>()));
            IFCDatabase.IFCHead.Insert(GetObjects(reader.Header.ToList<IFCBase>()));
            IFCDatabase.ReplaceTable.Insert(reader.ReplaceTable);
            IFCDatabase.InverseTable.Insert(reader.InverseTable);
        }
        private IEnumerable<BsonDocument> GetObjects(List<IFCBase> objs)
        {
            foreach (IFCBase data in objs)
                yield return data.ToBson();
        }
        public void Export(string folderPath, string fileName)
        {
            IFCExport export = new IFCExport(folderPath, fileName, IFCDatabase);
        }
        public void Close()
        {
            IFCDatabase.Close();
        }
    }
}
