using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCLite.Data
{
    public class IFCReplaceRecord:IFCSystemRecord
    {
        public string ValueElement { get; set; }
        public IFCReplaceRecord(string KeyElement) : base(KeyElement) { }
        public IFCReplaceRecord(string KeyElement, string ValueElement) : base(KeyElement)
        {
            this.ValueElement = ValueElement;
        }
    }
}
