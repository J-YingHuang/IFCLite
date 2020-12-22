using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace IFCLite.Data
{
    public class IFCHeader:IFCBase
    {
        public IFCHeader(BsonDocument Data) : base(Data) { }
        public override string ToIFCString()
        {
            string res = $"{EntityName} (";
            List<BsonValue> values = GetValues();
            res += $"{ValueToIFC(values[0])}";

            for (int i = 1; i < values.Count - 1; i++)
                res += $",{ValueToIFC(values[i])}";
            res += ");";
            return res;
        }
    }
}
