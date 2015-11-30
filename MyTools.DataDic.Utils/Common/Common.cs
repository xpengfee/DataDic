using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace MyTools.DataDic.Utils
{
    /// <summary>
    /// 公共方法
    /// </summary>
    public class Common
    {
        /// <summary>
        /// 获取列的数据类型
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>数据类型</returns>
        public static string GetColumnDataType(string str)
        {
            str = str.ToLower();
            if (str.Contains("dec"))
            {
                return "decimal";
            }
            else if (str.Contains("datetime"))
            {
                return "datetime";
            }
            else if (str.Contains("uniqueidentifier"))
            {
                return "uniqueidentifier";
            }
            else if (str.Contains("money"))
            {
                return "money";
            }
            else if (str.Contains("int"))
            {
                return str;
            }
            else if (str.Contains("char"))
            {
                return str.Split(new char[] { '(', '（' })[0];
            }
            else if (str.Contains("bit"))
            {
                return "bit";
            }
            else if (str.Contains("binary"))
            {
                return str.Split(new char[] { '(', '（' })[0];
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 获取列的宽度
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>列宽度</returns>
        public static string GetColumnWidth(string str)
        {
            str = str.ToLower();
            if (str.Contains("dec"))
            {
                return "0";
            }
            else if (str.Contains("datetime"))
            {
                return "40";
            }
            else if (str.Contains("uniqueidentifier"))
            {
                return "40";
            }
            else if (str.Contains("money"))
            {
                return "40";
            }
            else if (str.Contains("int"))
            {
                return "20";
            }
            else if (str.Contains("char"))
            {
                if (str.Contains("max"))
                {
                    return "-1";
                }
                return str.Split(new char[] { '(', '（' })[1].Replace(")", "").Replace("）", "");
            }
            else if (str.Contains("bit"))
            {
                return "1";
            }
            else if (str.Contains("binary"))
            {
                if (str.Contains("max"))
                {
                    return "-1";
                }
                return str.Split(new char[] { '(', '（' })[1].Replace(")", "").Replace("）", "");
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// 获取列的宽度
        /// </summary>
        /// <param name="strDataType">数据类型</param>
        /// <returns>列宽度</returns>
        public static string GetColumnWidth(string strDataType, string prec, string scale)
        {
            strDataType = strDataType.ToLower();

            if (strDataType.Contains("money") || strDataType.Contains("int") || strDataType == "uniqueidentifier" || strDataType.Contains("datetime"))
            {
                return "";
            }
            else if (strDataType == "decimal" || strDataType == "numeric")
            {
                return prec + "," + scale;
            }
            else if (strDataType.Contains("char"))
            {
                if (prec == "-1")
                {
                    return "max";
                }
                return prec;
            }
            else if (strDataType.Contains("bit") || strDataType.Contains("float") || strDataType.Contains("real"))
            {
                return "";
            }
            else if (strDataType.Contains("binary"))
            {
                if (prec == "-1")
                {
                    return "max";
                }
                return prec;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 导出文档获取数据库字段的默认值信息
        /// </summary>
        /// <param name="def">值</param>
        /// <param name="DataType">数据类型</param>
        /// <returns>默认值</returns>
        public static string GetDefaultValue(string def, string DataType)
        {
            if (string.IsNullOrEmpty(def))
            {
                return "";
            }
            //移除前后括号
            def = def.Remove(0, 1);
            def = def.Remove(def.Length - 1, 1);
            DataType = DataType.ToLower();
            switch (DataType)
            {
                case "uniqueidentifier":
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "datetime":
                case "text":
                    if (def.StartsWith("'"))
                    {
                        def = def.Remove(0, 1);
                        def = def.Remove(def.Length - 1, 1);
                    }
                    return def;
                default:
                    if (def.StartsWith("("))
                    {
                        def = def.Remove(0, 1);
                        def = def.Remove(def.Length - 1, 1);
                    }
                    return def;
            }
        }

        /// <summary>
        /// 导入文档获取数据库字段的默认值信息
        /// </summary>
        /// <param name="def">值</param>
        /// <param name="DataType">数据类型</param>
        /// <returns>默认值</returns>
        public static string ImportGetDefaultValue(string def, string DataType)
        {
            string temp = "";
            if (string.IsNullOrEmpty(def))
            {
                return temp;
            }
            DataType = DataType.ToLower();
            temp = def;
            switch (DataType)
            {
                case "uniqueidentifier":
                    if (!def.ToLower().Contains("id"))
                    {
                        temp = "'" + def + "'";
                    }
                    break;
                case "datetime":
                    if (!def.ToLower().Contains("date"))
                    {
                        temp = "'" + def + "'";
                    }
                    break;
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                    temp = "'" + def + "'";
                    break;
                default:
                    temp= def;
                    break;
            }
            return temp;
        }


        /// <summary>
        /// 判断一个对象是否为True
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>Boolean</returns>
        public static Boolean ConvertToBooleanPG(Object obj)
        {
            if (obj != null)
            {
                string mStr = obj.ToString();
                mStr = mStr.ToLower();
                if ((mStr.Equals("y") || mStr.Equals("1")) || mStr.Equals("true") || mStr.Equals("是"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断一个连接字符串是否正确
        /// </summary>
        /// <param name="constr">数据库连接字符串</param>
        /// <returns>Boolean</returns>
        public static Boolean IsCorrectConnection(string constr)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取数据字段类型
        /// </summary>
        /// <param name="DataType">数据字段类型</param>
        /// <param name="width">数据字段宽度</param>
        /// <returns>数据字类型段</returns>
        public static string GetDataTypeStr(string DataType, string width)
        {
            try
            {
                DataType = DataType.ToLower();
                string strResult = DataType;
                if (!DataType.Contains("("))
                {
                    if (!string.IsNullOrEmpty(width))
                    {
                        if (DataType.Contains("dec") || DataType.Contains("char") || DataType.Contains("binary"))
                        {
                            strResult = DataType + "(" + width.Trim() + ")";
                        }
                    }
                }
                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 获取数据字段宽度
        /// </summary>
        /// <param name="DataType">数据字段类型</param>
        /// <param name="width">数据字段宽度</param>
        /// <returns>数据字类型段</returns>
        public static string GetColWidth(string DataType, string width)
        {
            try
            {
                DataType = DataType.ToLower();
                string strResult = width;
                if (!DataType.Contains("("))
                {

                    if (DataType.Contains("dec") || DataType.Contains("char") || DataType.Contains("binary"))
                    {
                        if (string.IsNullOrEmpty(width))
                        {
                            if (DataType.Contains("dec"))
                            {
                                strResult = "20,2";
                            }
                            else
                            {
                                strResult = "1000";
                            }
                        }
                    }
                }
                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取EXECL或者Word的列信息
        /// </summary>
        /// <param name="dic">列信息字典信息</param>
        /// <param name="sTemp">列的值</param>
        /// <param name="mColumn">列</param>
        /// <returns>列信息</returns>
        public static void GetColumnInfo(Dictionary<int, string> dic, string sTemp, ColumnInfo mColumn, int iCell, TableInfo pTable)
        {
            switch (dic[iCell])
            {
                //列中文名称
                case "字段中文名":
                    mColumn.Name = sTemp;
                    break;
                //列英文名称
                case "字段英文名":
                    mColumn.Code = sTemp;
                    break;
                case "数据类型":
                    mColumn.DataTypeStr = sTemp.Replace("（", "(").Replace("）", ")");
                    mColumn.DataType = Common.GetColumnDataType(mColumn.DataTypeStr);
                    break;
                case "宽度":
                    mColumn.Width = sTemp;
                    break;
                //主键信息
                case "主键":
                case "约束":
                    if (sTemp.ToLower() == "pk" || Common.ConvertToBooleanPG(sTemp))
                    {
                        PkKeyInfo pk = new PkKeyInfo();
                        pk.Name = mColumn.Code;
                        mColumn.PK = true;
                        pTable.ListPkKeyInfo.Add(pk);
                    }
                    break;
                //列默认值
                case "默认值":
                    mColumn.DefaultValue = sTemp;
                    break;
                //列描述
                case "说明":
                case "枚举&说明":
                    mColumn.Comment = sTemp;
                    break;
                //列是否可为空
                case "空值":
                    //是否为空值处理和其它的不相同,只会填N或0表示该列不允许为空
                    sTemp = sTemp.ToLower();
                    if ((sTemp.Equals("n") || sTemp.Equals("0")) || sTemp.Equals("false") || sTemp.Equals("否"))
                    {
                        mColumn.Nullable = true;
                    }
                    else
                    {
                        mColumn.Nullable = false;
                    }
                    break;
                //列是否自增
                case "自增":
                    mColumn.Identity = Common.ConvertToBooleanPG(sTemp);
                    break;
                default:
                    break;
            }
        }
    }
}
