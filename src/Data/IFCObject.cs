using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiteDB;

namespace IFCLite.Data
{
    public class IFCObject:IFCBase
    {
        public IFCObject(BsonDocument Data) : base(Data) { }
        //Basic Prop Access
        /// <summary>
        /// Get P21Id of IFCObject.
        /// </summary>
        /// <returns></returns>
        public string P21Id
        {
            get { return Data["P21Id"].AsString; }
            set { Data["P21Id"] = value; }
        }
        //For Access Data
        public List<string> GetFields()
        {
            List<string> res = new List<string>();
            foreach (var prop in Data)
                res.Add(prop.Key);
            res.Remove("P21Id");
            res.Remove("oid");
            res.Remove("EntityName");
            return res;
        }
        public BsonValue Get(string filed)
        {
            return Data[filed];
        }
        public bool Set(string field, BsonValue value)
        {
            BsonValue oriValue = Data[field];
            if (oriValue.Type != value.Type)
                return false;
            Data[field] = value;
            return true;
        }
        public override string ToIFCString()
        {
            string res = $"{P21Id}={EntityName}(";
            List<BsonValue> values = GetValues();
            res += $"{ValueToIFC(values[0])}";

            for (int i = 1; i < values.Count - 1; i++)
                res += $",{ValueToIFC(values[i])}";
            res += ");";
            return res;
        }
    }
}
