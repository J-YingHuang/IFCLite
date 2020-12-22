using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCLite.Data
{
    public class IFCInverseRecord:IFCSystemRecord
    {
        public List<string> ValueElement { get; set; }
        public IFCInverseRecord(string KeyElement) : base(KeyElement) { ValueElement = new List<string>(); }
        public IFCInverseRecord(string KeyElement, List<string> ValueElement) : base(KeyElement)
        {
            this.ValueElement = ValueElement;
        }
    }
}
