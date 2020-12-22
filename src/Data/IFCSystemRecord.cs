using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFCLite.Data
{
    public abstract class IFCSystemRecord
    {
        public string KeyElement { get; set; }
        public IFCSystemRecord(string KeyElement)
        {
            this.KeyElement = KeyElement;
        }
    }
}
