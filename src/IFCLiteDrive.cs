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
    /// <summary>
    /// IFCLite主要物件，負責處理所有IFCDatabase事項。
    /// </summary>
    public class IFCLiteDrive
    {
        private IFCDatabase IFCDatabase { get; set; }
        private SchemaReader SchemaReader { get; set; }
        /// <summary>
        /// Basic query module.
        /// </summary>
        public IFCQuery Query { get { return new IFCQuery(IFCDatabase); } }
        /// <summary>
        /// Basic insert module.
        /// </summary>
        public IFCInsert Insert { get { return new IFCInsert(IFCDatabase); } }
        /// <summary>
        /// Basic remove module.
        /// </summary>
        public IFCDelete Remove { get { return new IFCDelete(IFCDatabase); } }
        /// <summary>
        /// Basic update module.
        /// </summary>
        public IFCUpdate Update { get { return new IFCUpdate(IFCDatabase); } }
        /// <summary>
        /// Create new IFCLiteDrive and import Ifc file.
        /// </summary>
        /// <param name="dbPath">Path of LiteDB.</param>
        /// <param name="ifcPath">Path of IFC file.</param>
        /// <param name="version">Version of IFC.</param>
        /// <param name="create">If create index.</param>
        public IFCLiteDrive(string dbPath, string ifcPath, IFCVersion version, bool create)
        {
            SchemaReader = new SchemaReader(version);
            IFCDatabase = new IFCDatabase(dbPath,create);
            AddDataToDatabase(ifcPath);
        }
        /// <summary>
        /// Create new IFCLiteDrive.
        /// </summary>
        /// <param name="dbPath">Path of LiteDB.</param>
        /// <param name="version">Version of IFC file.</param>
        /// <param name="create">If create index.</param>
        public IFCLiteDrive(string dbPath, IFCVersion version, bool create)
        {
            SchemaReader = new SchemaReader(version);
            IFCDatabase = new IFCDatabase(dbPath, create);
        }
        private void AddDataToDatabase(string ifcpath)
        {
            IFCReader reader = new IFCReader(ifcpath, SchemaReader);
            Insert.IFCObject(reader.InsertObjs);
            Insert.IFCHeadInsert(reader.Header);
            IFCDatabase.ReplaceTable.Insert(reader.ReplaceTable);
            IFCDatabase.InverseTable.Insert(reader.InverseTable);
        }
        private IEnumerable<BsonDocument> GetObjects(List<IFCBase> objs)
        {
            foreach (IFCBase data in objs)
                yield return data.ToBson();
        }
        /// <summary>
        /// Export IFC file from database.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="fileName">File name.</param>
        public void Export(string folderPath, string fileName)
        {
            IFCExport export = new IFCExport(folderPath, fileName, IFCDatabase);
        }
        /// <summary>
        /// Database disconnect.
        /// </summary>
        public void Close()
        {
            IFCDatabase.Close();
        }
    }
}
