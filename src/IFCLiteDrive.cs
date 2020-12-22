using System;
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
            foreach (IFCObject obj in reader.InsertObjs)
                IFCDatabase.IFCModel.Insert(obj.ToBson());
            foreach (IFCHeader header in reader.Header)
                IFCDatabase.IFCHead.Insert(header.ToBson());
            foreach (IFCReplaceRecord record in reader.ReplaceTable)
                IFCDatabase.ReplaceTable.Insert(record);
            foreach (IFCInverseRecord inverse in reader.InverseTable)
                IFCDatabase.InverseTable.Insert(inverse);
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
