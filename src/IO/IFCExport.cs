using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFCLite.Data;
using LiteDB;

namespace IFCLite.IO
{
    public class IFCExport
    {
        /// <summary>
        /// Key 取代者, Value 被取代者
        /// </summary>
        private Dictionary<string, List<string>> ReplaceTable { get; set; }
        public IFCExport(string folderPath, string fileName, IFCDatabase db)
        {
            GetReplaceData(db);
            using(StreamWriter sw = new StreamWriter($"{folderPath}\\{fileName}.ifc", false))
            {
                foreach (string data in GetIFCFileString(db))
                    sw.WriteLine(data);
            }
        }
        /// <summary>
        /// 取得Replace Table資料
        /// </summary>
        /// <param name="db"></param>
        public void GetReplaceData(IFCDatabase db)
        {
            ReplaceTable = new Dictionary<string, List<string>>();
            foreach(IFCReplaceRecord data in db.ReplaceTable.FindAll())
            {
                if (ReplaceTable.ContainsKey(data.ValueElement))
                    ReplaceTable[data.ValueElement].Add(data.KeyElement);
                else
                    ReplaceTable.Add(data.ValueElement, new List<string>() { data.KeyElement }); 
            }
        }
        public List<string> GetIFCFileString(IFCDatabase db)
        {
            List<string> res = new List<string>();
            res.Add("ISO-10303-21;");
            res.Add(GetHeaderData(db));
            res.AddRange(GetData(db));
            return res;
        }
        public string GetHeaderData(IFCDatabase db)
        {
            string space = "\r\n";
            string res = $"HEADER;{space}";
            IFCHeader des = new IFCHeader(db.IFCHead.FindOne(x => x["EntityName"] == "FILE_DESCRIPTION"));
            IFCHeader name = new IFCHeader(db.IFCHead.FindOne(x => x["EntityName"] == "FILE_NAME"));
            IFCHeader schema = new IFCHeader(db.IFCHead.FindOne(x => x["EntityName"] == "FILE_SCHEMA"));
            return res + des.ToIFCString() + space + name.ToIFCString() + space + schema.ToIFCString() + space + "ENDSEC;" + space;
        }
        public List<string> GetData(IFCDatabase db)
        {
            Dictionary<string, string> objDict = new Dictionary<string, string>();
            foreach (BsonDocument obj in db.IFCModel.FindAll())
                objDict.Add(obj["P21Id"].AsString, new IFCObject(obj).ToIFCString());

            foreach(var pair in ReplaceTable)
            {
                string data = objDict[pair.Key];
                data = data.Substring(data.IndexOf('=') + 1, data.IndexOf(';') - data.IndexOf('=') );
                foreach (string replaceId in pair.Value)
                    objDict.Add(replaceId, $"{replaceId}={data}");
            }

            Dictionary<int, string> sortData = objDict.ToDictionary(o => int.Parse(o.Key.Replace("#", "")), p => p.Value);
            sortData = sortData.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            List<string> res = new List<string>() { $"DATA;" };
            foreach (string data in sortData.Values)
                res.Add($"{data}");
            res.Add("ENDSEC;");
            res.Add("END-ISO-10303-21;");
            return res;
        }
    }
}
