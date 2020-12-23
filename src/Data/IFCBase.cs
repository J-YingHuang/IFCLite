using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace IFCLite.Data
{
    public abstract class IFCBase
    {
        protected BsonDocument Data { get; set; }
        /// <summary>
        /// Get EntityName
        /// </summary>
        /// <returns></returns>
        public string EntityName
        {
            get { return Data["EntityName"].AsString; }
        }

        public IFCBase(BsonDocument Data)
        {
            this.Data = Data;
            this.Data.Remove("_id"); //刪除oid
        }
        public BsonDocument ToBson()
        {
            return Data;
        }
        public List<BsonValue> GetValues()
        {
            List<BsonValue> res = new List<BsonValue>();
            foreach (var prop in Data)
            {
                if (prop.Key == "P21Id") continue;
                if (prop.Key == "EntityName") continue;
                res.Add(prop.Value);
            }
            return res;
        }
        protected string GetIFCValueString(string value)
        {
            if (value == "$")
                return value;
            Regex p21idPattern = new Regex(@"^#[0-9]*[0-9]$");
            if (p21idPattern.IsMatch(value))
                return value;
            Regex numberPattern = new Regex(@"^[-]*[0-9]*.[0-9]*$");
            if (numberPattern.IsMatch(value))
                return value;
            Regex earthNumberPattern = new Regex(@"^[-]*[0-9]*.[0-9]*E[+-]*[0-9]*[0-9]$");
            if (earthNumberPattern.IsMatch(value))
                return value;
            Regex enumPattern = new Regex(@"^.[\S]*[.]$");
            if (enumPattern.IsMatch(value))
                return value;
            Regex ifcObjPattern = new Regex(@"^IFC[A-Z]*\([\S\s]*\)$");
            if (ifcObjPattern.IsMatch(value))
                return value;
            if (value == "")
                return "''";
            return (value[0] == '\'' && value[value.Length - 1] == '\'') ? ConvertToIFCString(value) : $"'{ConvertToIFCString(value)}'";
        }
        protected bool IsChinese(string _String)
        {
            return (_String != string.Empty && !Regex.IsMatch(_String, @"^[\u4e00-\u9fa5]{0,}$"))
                ? false : true;
        }
        protected string ToUnicode(char[] _Code)
        {
            string unicode = "";

            foreach (char v in _Code)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(v.ToString());
                unicode += string.Format("{0:X}", bytes[1]) + string.Format("{0:X}", bytes[0]);
            }

            return unicode;
        }
        protected string ConvertToIFCString(string _String)
        {
            string res = "";
            string temp = "";
            foreach (char s in _String)
                if (IsChinese(s.ToString())) //是否為中文字
                {
                    temp += s; //將中文字暫時存放在temp字串中
                    continue; //執行下一圈
                }
                else
                {
                    if (temp != "") //暫存字串若具有未處理的中文字
                    {
                        res += @"\X2\" + ToUnicode(temp.ToCharArray()) + @"\X0\"; //將暫存的中文字轉為編碼
                        temp = ""; //清空暫存字元
                    }
                    res += s;
                }

            if (temp != "")
                res += @"\X2\" + ToUnicode(temp.ToCharArray()) + @"\X0\"; //將暫存的中文字轉為編碼

            return res;
        }
        protected string ValueToIFC(BsonValue value)
        {
            if (value.Type == BsonType.String)
                return GetIFCValueString(value.AsString);
            BsonArray array = value as BsonArray;
            if (array.Count == 0)
                return "()";

            //內容至少有一個的陣列
            string res = $"({GetIFCValueString(array[0].AsString)}";
            for (int i = 1; i < array.Count; i++)
                res += $",{GetIFCValueString(array[i].AsString)}";
            res += ")";
            return res;
        }
        public virtual string ToIFCString()
        {
            return "";
        }
    }
}
