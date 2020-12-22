using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCLite.IO
{
    internal class IFCData
    {
        public string P21Id { get; set; }
        public string EntityName { get; set; }
        public string Properties { get; set; }

        public IFCData() { }
        public IFCData(string P21Id, string EntityName, string Properties)
        {
            this.P21Id = P21Id.Trim();
            this.EntityName = EntityName.Trim();
            this.Properties = Properties.Trim();
        }
        public List<string> GetPropsArray()
        {
            return Properties.Split(',').ToList();
        }
    }
}
