using IFCLite.Data;
using IFCLite.Data.Schema;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IFCLite.IO
{
    public class IFCReader
    {
        private SchemaReader SchemaReader;
        public string ResultMessage { get; private set; }
        public List<IFCObject> InsertObjs { get; private set; }
        public List<IFCHeader> Header { get; private set; }
        public List<IFCReplaceRecord> ReplaceTable { get; private set; }
        public List<IFCInverseRecord> InverseTable { get; private set; }

        public IFCReader(string filePath, SchemaReader schema)
        {
            SchemaReader = schema;
            Header = new List<IFCHeader>();
            ReplaceTable = new List<IFCReplaceRecord>();
            InverseTable = new List<IFCInverseRecord>();
            Dictionary<string, IFCObject> dist = CombineIFC(SplitIFCFile(filePath));
            InsertObjs = dist.Values.ToList();
            FindInverseData(dist);
        }
        private List<IFCData> SplitIFCFile(string filepath)
        {
            List<IFCData> allIFCRow = new List<IFCData>();
            StreamReader reader = new StreamReader(filepath);
            string lastString = "";
            int countOfStream = 0;      //行數計算
            int countOfAllString = 0;   //字元計算
            if (!reader.EndOfStream)
            {
                string headValue = reader.ReadLine();
            }
            while (!reader.EndOfStream)
            {
                string tmp = lastString + reader.ReadLine();
                //跳過/* */註解行
                if (tmp == "" || (tmp.Substring(0, 2) == "/*" || tmp[0] == '*' || tmp.Substring(tmp.Length - 2) == "*/"))
                    continue;
                if (tmp[tmp.Length - 1] != ';')
                {
                    lastString = tmp;
                    continue;           //跳下一行
                }

                countOfStream++;
                countOfAllString += tmp.Length;
                if (tmp.Contains('=') && tmp.Contains(')') && tmp.Contains('('))    //表示完整的一行ifc資料
                {
                    string ifcHash = tmp.Substring(0, tmp.IndexOf('=')).Trim(); //刪除空白
                    string ifcName = tmp.Substring(tmp.IndexOf('=') + 1, tmp.IndexOf('(') - tmp.IndexOf('=') - 1).Trim(); //刪除空白
                    string ifcContent = tmp.Substring(tmp.IndexOf('(') + 1, tmp.LastIndexOf(')') - tmp.IndexOf('(') - 1).Trim(); //刪除空白
                    allIFCRow.Add(new IFCData(ifcHash, ifcName, ifcContent));
                }

                if (tmp.Contains("FILE_DESCRIPTION"))
                {
                    string ifcContent = tmp.Substring(tmp.IndexOf('(') + 1, tmp.LastIndexOf(')') - tmp.IndexOf('(') - 1).Trim(); //刪除空白
                    List<string> prop = SplitProperty(CutStringWithComma(ifcContent));
                    BsonDocument file_des = new BsonDocument();
                    file_des.Add("EntityName", "FILE_DESCRIPTION");
                    if (prop[0] == "()")
                        file_des.Add("description", "$");
                    else
                        file_des.Add("description", GetArray(prop[0]));
                    file_des.Add("implementation_level", GetValue(prop[1]));
                    Header.Add(new IFCHeader(file_des));
                }

                if (tmp.Contains("FILE_NAME"))
                {
                    string ifcContent = tmp.Substring(tmp.IndexOf('(') + 1, tmp.LastIndexOf(')') - tmp.IndexOf('(') - 1).Trim(); //刪除空白
                    List<string> prop = SplitProperty(CutStringWithComma(ifcContent));
                    BsonDocument file_name = new BsonDocument
                    {
                        { "EntityName", "FILE_NAME" }
                    };
                    List<string> itemName = new List<string>()
                    {
                        "name", "time_stamp","author","organization","preprocessor_version","originating_system","authorization"
                    };
                    for (int i = 0; i < prop.Count(); i++)
                    {
                        if (prop[i] == "")
                        {
                            file_name.Add(itemName[i], "");
                            continue;
                        }
                        if (prop[i].Substring(0, 1) != "(")
                        {
                            file_name.Add(itemName[i], GetValue(prop[i]));
                            continue;
                        }
                        file_name.Add(itemName[i], GetArray(prop[i]));
                    }
                    Header.Add(new IFCHeader(file_name));
                }

                if (tmp.Contains("FILE_SCHEMA"))
                {
                    string ifcContent = tmp.Substring(tmp.IndexOf('(') + 1, tmp.LastIndexOf(')') - tmp.IndexOf('(') - 1).Trim(); //刪除空白
                    BsonDocument file_schema = new BsonDocument
                    {
                        { "EntityName", "FILE_SCHEMA" }
                    };
                    file_schema.Add("schema_identifiers", GetArray(ifcContent));
                    Header.Add(new IFCHeader(file_schema));
                    //ResultMessage += "使用" + schemaType + Environment.NewLine;
                }

                lastString = "";        //處理完畢，清空上一行
            }
            return allIFCRow;
        }
        private Dictionary<string, IFCObject> CombineIFC(List<IFCData> allIFCRow)
        {
            Dictionary<string, IFCObject> objDist = new Dictionary<string, IFCObject>();
            foreach (IFCData data in FilerReplaceData(allIFCRow))
            //foreach (IFCData data in allIFCRow) //無Replace
            {
                //try
                //{
                BsonDocument obj = new BsonDocument
                    {
                        { "P21Id", data.P21Id },
                        { "EntityName", data.EntityName }
                    };
                //Get Schema
                List<string> schemaList = SchemaReader.GetAttributesList(data.EntityName);
                //處理IFC原始字串編碼轉換, 並排除字串中有逗號的錯誤
                List<string> contentSplit = SplitProperty(CutStringWithComma(ConvertUnicodeStringToChinese(data.Properties)));

                for (int i = 0; i < contentSplit.Count; i++)
                {
                    if (contentSplit[i] == "") //空字串
                    {
                        obj.Add(schemaList[i], "");
                        continue;
                    }
                    if (contentSplit[i].Substring(0, 1) != "(") //非陣列可直接儲存
                    {
                        obj.Add(schemaList[i], GetValue(contentSplit[i]));
                        continue;
                    }
                    BsonArray arr1 = GetArray(contentSplit[i]);
                    obj.Add(schemaList[i], GetArray(contentSplit[i]));
                }
                objDist.Add(data.P21Id, new IFCObject(obj));
                //}
                //catch (Exception exp)
                //{
                //    ResultMessage += $"Has Error: {exp.Message}";
                //}
            }
            return objDist;
        }
        private BsonValue GetValue(string data)
        {
            if (data.Contains(".") && double.TryParse(data, out double numberData)) //雙精浮點數
                return new BsonValue(numberData);
            if (Int64.TryParse(data, out Int64 intData)) //整數
                return new BsonValue(intData);

            Regex stringPattern = new Regex(@"^'[\S\s]*'$"); //字串形式
            if (stringPattern.IsMatch(data)) //字串 
                return new BsonValue(CutString(data));
            else
                return new BsonValue(data); //ENUM
        }
        private void FindInverseData(Dictionary<string, IFCObject> dist)
        {
            Dictionary<string, List<string>> inverseData = new Dictionary<string, List<string>>();
            foreach (IFCObject data in dist.Values)
            {
                foreach (BsonValue prop in data.GetValues())
                {
                    string value = "";
                    if (prop.Type == BsonType.String)
                        value = prop.AsString;
                    if (prop.Type == BsonType.Array && (prop as BsonArray).Count != 0)
                    {
                        BsonValue propData = (prop as BsonArray).First();
                        while (propData.Type == BsonType.Array)
                            propData = (propData as BsonArray).First();
                        if (propData.Type != BsonType.String)
                            continue;
                        else
                            value = propData.AsString;
                    }

                    if (!value.Contains("#")) //不含#代表沒有上下層關係則不處理
                        continue;
                    if (dist.ContainsKey(value)) //含該物件
                    {
                        IFCObject downObj = dist[value];
                        if (inverseData.ContainsKey(downObj.EntityName))
                        {
                            if (!inverseData[downObj.EntityName].Contains(data.EntityName))
                                inverseData[downObj.EntityName].Add(data.EntityName);
                        }
                        else
                            inverseData.Add(downObj.EntityName, new List<string>() { data.EntityName });
                    }
                }
            }
            foreach (var data in inverseData)
                InverseTable.Add(new IFCInverseRecord(data.Key, data.Value));
        }
        private List<IFCData> FilerReplaceData(List<IFCData> allIFCRow)
        {
            allIFCRow = allIFCRow.OrderBy(o => o.Properties).ToList();
            List<string> redundID = new List<string>(); //被替換之P21ID
            List<IFCData> combainData = new List<IFCData>();
            for (int i = 0; i < allIFCRow.Count; i++)
            {
                IFCData nowData = allIFCRow[i];
                if (redundID.Contains(nowData.P21Id)) //已被取代之IFC資料不處理
                    continue;
                combainData.Add(nowData);
                for (int j = i + 1; j < allIFCRow.Count; j++)
                {
                    if (nowData.Properties == allIFCRow[j].Properties)
                    {
                        ReplaceTable.Add(new IFCReplaceRecord(allIFCRow[j].P21Id, nowData.P21Id)); //被替換者-保留者
                        redundID.Add(allIFCRow[j].P21Id);
                    }
                    else
                        break;
                }
            }
            return combainData;
        }
        private List<string> CutStringWithComma(string values)
        {
            List<string> returnList = new List<string>();

            bool inString = false;
            string tmpString = "";
            int valueCharNum = values.Length;
            int nowLoopNum = 1;
            foreach (char s in values)
            {
                switch (s.ToString())
                {
                    case "'":
                        //確認是否在字串內
                        if (inString)
                            inString = false;
                        else
                            inString = true;

                        tmpString += s;
                        break;
                    case ",":
                        //確認是否在字串中, 如果是在字串中的逗號就補回去字串, 不是的話代表是間隔值與值之間的逗號,需要將值加入陣列中,並重新存值
                        if (inString)
                            tmpString += s;
                        else
                        {
                            returnList.Add(tmpString);
                            tmpString = "";
                        }
                        break;
                    default:
                        tmpString += s;
                        break;
                }

                if (nowLoopNum == valueCharNum) //確認是否已達最後一個迴圈,是就要把殘存的最後一個值加進去字串裡面
                    returnList.Add(tmpString);
                else
                    nowLoopNum++;
            }

            return returnList;
        }
        private List<string> SplitProperty(List<string> commaSplit)
        {
            List<string> res = new List<string>();
            string tmpString = "";                                  //用來"接"字串
            int indexOfSchema = 0;                                  //用來對應aIFCRowData裡面的index, 跳過第一個type
            string arrayEndString = "";
            bool isArrayValue = false;
            bool isStringValue = false;
            try
            {
                foreach (string s in commaSplit)
                {
                    string cutS = s.Trim();
                    if (isArrayValue || isStringValue)
                        tmpString += "," + cutS;                             //把逗點補回去
                    else
                        tmpString += cutS;
                    if (cutS[0] == '(')
                    {                                                        //ifc的值是陣列  
                        if (!isArrayValue) //第一次碰到陣列才須要讀結束的符號
                            for (int i = 0; i < cutS.Length; i++)
                            {
                                if (cutS[i] == '(')
                                    arrayEndString += ")"; //對應的陣列符號
                                else
                                    break;
                            }

                        if (cutS.LastIndexOf(arrayEndString) != -1)    //該陣列找的到結尾陣列符號
                        {
                            res.Add(tmpString.Trim());
                            arrayEndString = "";
                            tmpString = "";
                            indexOfSchema++;
                        }
                        else
                            isArrayValue = true;
                    }
                    else if (cutS[0] == '\'')
                    {
                        if (isArrayValue && cutS.LastIndexOf(arrayEndString) != -1)
                        {
                            res.Add(tmpString.Trim());
                            tmpString = "";
                            indexOfSchema++;
                            isArrayValue = false;
                            arrayEndString = "";
                            continue;
                        }
                        if (cutS[cutS.Length - 1] == '\'')                     //結尾就是'\''
                        {
                            res.Add(tmpString);
                            tmpString = "";
                            indexOfSchema++;
                        }
                        else
                            isStringValue = true;
                    }
                    else
                    {
                        if (isArrayValue && !isStringValue)
                        {
                            if (cutS.LastIndexOf(arrayEndString) != -1)                //陣列結尾         else 僅連結陣列字串(開頭就做了)
                            {
                                isArrayValue = false;
                                res.Add(tmpString.Trim());
                                tmpString = "";
                                arrayEndString = "";
                                indexOfSchema++;
                            }
                        }
                        else if (!isArrayValue && isStringValue)       //only one situation could happened   array or string
                        {
                            if (cutS[cutS.Length - 1] == '\'')               //字串結尾         else 僅連結陣列字串(開頭就做了)
                            {
                                if (cutS.Length > 2)
                                    if (cutS[cutS.Length - 2] == '\\')        //s的長度如果小於2會出現runtime error
                                        continue;
                                isStringValue = false;
                                res.Add(tmpString);
                                tmpString = "";
                                indexOfSchema++;
                            }
                        }
                        else
                        {
                            res.Add(tmpString.Trim());
                            indexOfSchema++;
                            tmpString = "";
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Console.Write(exp.Message);
            }
            return res;
        }
        private string CutString(string tmp)
        {
            string res = tmp.Trim().Substring(1);
            return res.Substring(0, res.Length - 1);
        }
        private BsonArray GetArray(string data)
        {
            BsonArray arr = new BsonArray();
            List<string> cutData = SplitProperty(CutStringWithComma(CutString(data)));
            foreach (string val in cutData)
            {
                if (val == "") //空字串{
                {
                    arr.Add("");
                    continue;
                }
                if (val.Substring(0, 1) != "(") //非陣列可直接儲存
                {
                    arr.Add(GetValue(val));
                    continue;
                }
                arr.Add(GetArray(val)); //遇到陣列須再執行一次
            }
            return arr;
        }
        #region --unicode--
        private string ConvertUnicodeStringToChinese(string _String)
        {
            if (!_String.Contains(@"\X2\"))
                return _String;

            string oldString = _String;
            string res = "";
            do
            {
                int staIndex = oldString.IndexOf(@"\X2\");

                if (staIndex != 0)
                {
                    res += oldString.Substring(0, staIndex);
                    oldString = oldString.Substring(staIndex, oldString.Count() - staIndex);
                    staIndex = oldString.IndexOf(@"\X2\");
                }

                int endIndex = oldString.IndexOf(@"\X0\");

                string uniString = oldString.Substring(staIndex, endIndex + 4);
                oldString = oldString.Remove(0, endIndex + 4);

                uniString = uniString.Replace(@"\X2\", "").Replace(@"\X0\", "");
                for (int i = 0; i < uniString.Count(); i = i + 4)
                    res += ConvertToString(uniString.Substring(i, 4));

            } while (oldString.Contains(@"\X2\"));

            if (oldString != "")
                res += oldString;

            return res;
        }
        private string ConvertToString(string _String)
        {
            byte[] bytes = new byte[2];
            bytes[1] = Convert.ToByte(_String.Substring(0, 2), 16);
            bytes[0] = Convert.ToByte(_String.Substring(2, 2), 16);

            return Encoding.Unicode.GetString(bytes);
        }
        #endregion
    }
}
