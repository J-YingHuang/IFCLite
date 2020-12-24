using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.IO;
using System.Reflection;

namespace IFCLite.Data.Schema
{
    public class SchemaReader
    {
        private Dictionary<string, List<string>> SchemaDictionary { get; }
        public SchemaReader(IFCVersion useVersion)
        {
            SchemaDictionary = new Dictionary<string, List<string>>();
            ReadSchema(GetSchemaString(useVersion));
        }
        /// <summary>
        /// Parse IFC4 SchemaData
        /// </summary>
        /// <param name="schemaString"></param>
        private void ReadSchema(string schemaString)
        {
            JObject allSchema = JObject.Parse(schemaString);
            JArray schema = allSchema.GetValue("Schema") as JArray;
            foreach (JObject obj in schema)
            {
                List<string> props = new List<string>();
                foreach (var prop in obj.GetValue("Attributes"))
                    props.Add(prop.ToString());
                SchemaDictionary.Add(obj.GetValue("EntityName").ToString().ToUpper(), props);
            }
        }
        private string GetSchemaString(IFCVersion version)
        {
            string sourceName = "";
            Assembly assembly = Assembly.GetExecutingAssembly();
            switch (version)
            {
                case IFCVersion.IFC4:
                    sourceName = "IFCLite.Properties.IFC4ADD2.json";
                    break;
            }
            using (Stream stream = assembly.GetManifestResourceStream(sourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        public List<string> GetAttributesList(string entityName)
        {
            return SchemaDictionary[entityName.ToUpper()];
        }

    }
}
