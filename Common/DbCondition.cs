using System;
using System.Text;
using System.Collections;
using System.Data.SqlClient;

namespace Ro.Data.SqlClient
{	    
    public class DbCondition : Hashtable
    {
        private static string whereTag = " WHERE ";
        private static string equalTag = " {0}={1} ";

        private static string andEqualTag = " AND {0}={1} ";
        private static string orEqualTag = " OR {0}={1} ";

        private static string andNotEqualTag = " AND {0}<>{1} ";
        private static string orNotEqualTag = " OR {0}<>{1} ";

        private static string inTag = " {0} IN ({1}) ";
        private static string andInTag = " AND {0} IN ({1}) ";
        private static string orInTag = " OR {0} IN ({1}) ";

        private static string greaterTag = " {0}>{1} ";
        private static string greaterEqualTag = " {0}>={1} ";

        private static string andGreaterTag = " AND {0}>{1} ";
        private static string andGreaterEqualTag = " AND {0}>={1} ";

        private static string orGreaterTag = " OR {0}>{1} ";
        private static string orGreaterEqualTag = " OR {0}>={1} ";

        private static string lessTag = " {0}<{1} ";
        private static string lessEqualTag = " {0}<={1} ";

        private static string andLessTag = " AND {0}<{1} ";
        private static string andLessEqualTag = " AND {0}<={1} ";
       
        private static string orLessTag = " OR {0}<{1} ";
        private static string orLessEqual = " OR {0}<={1} ";
        
        private static string orderByAscTag = " ORDER BY {0} ASC ";
        private static string orderByDescTag = " ORDER BY {0} DESC ";

		 
        private static string paramChar = "@";
     
        private StringBuilder sbSQL = new StringBuilder();
		
        public string queryString = string.Empty;
		 
        public DbCondition Query(string query)
        {
            this.queryString = query;
            sbSQL.Append(query);
            return this;
        }

        public DbCondition Where()
        {          
            sbSQL.AppendFormat(whereTag + equalTag,1,1);
            return this;
        }
		 
        public DbCondition Where(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(whereTag + equalTag, fieldName, paramChar + formatName);      
            return this;
        }

        public DbCondition AndLeftBracket()
        {
            sbSQL.AppendFormat(" And ( ");
            return this;
        }

        public DbCondition OrLeftBracket()
        {
            sbSQL.AppendFormat(" Or ( ");
            return this;
        }

        public DbCondition RightBracket()
        {
            sbSQL.AppendFormat(" ) ");
            return this;
        }

		
        public DbCondition Equal(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(equalTag, fieldName, paramChar + formatName);   
            return this;
        }	
        	
        public DbCondition AndEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(andEqualTag, fieldName, paramChar + formatName);
            return this;
        }

        public DbCondition AndNotEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(andNotEqualTag, fieldName, paramChar + formatName);
            return this;
        }

        public DbCondition OrEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(orEqualTag, fieldName, paramChar + formatName);
            return this;
        }

        public DbCondition OrNotEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(orNotEqualTag, fieldName, paramChar + formatName);
            return this;
        }

        public DbCondition In(string fieldName, int[] fieldValue)
        {
            string formatName = FormatKey(fieldName);
            sbSQL.AppendFormat(inTag, fieldName, string.Join(",", fieldValue));
            return this;
        }

        public DbCondition AndIn(string fieldName, int[] fieldValue)
        {            
            string formatName = FormatKey(fieldName);             
            sbSQL.AppendFormat(andInTag, fieldName, string.Join(",", fieldValue));
            return this;
        }

        public DbCondition OrIn(string fieldName, int[] fieldValue)
        {
            string formatName = FormatKey(fieldName);
            sbSQL.AppendFormat(orInTag, fieldName, string.Join(",", fieldValue));
            return this;
        }

        public DbCondition GreaterThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(greaterTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition GreaterThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(greaterEqualTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition AndGreaterThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(andGreaterTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition AndGreaterThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(andGreaterEqualTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition OrGreaterThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(orGreaterTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition OrGreaterThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(orGreaterEqualTag, fieldName, paramChar + formatName);
            return this;
        }
 
        public DbCondition LessThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(lessTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition LessThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(lessEqualTag, fieldName, paramChar + formatName);
            return this;
        }
 
        public DbCondition AndLessThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(andLessTag, fieldName, paramChar + formatName);
            return this;
        }
         
        public DbCondition AndLessThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(andLessEqualTag, fieldName, paramChar + formatName);
            return this;
        }
	 
        public DbCondition OrLessThan(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(orLessTag, fieldName, paramChar + formatName);
            return this;
        }
		 
        public DbCondition OrLessThanEqual(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);  
            sbSQL.AppendFormat(orLessEqual, fieldName, paramChar + formatName);
            return this;
        }
	 
        public DbCondition And(string fieldName, object fieldValue)
        {
            return this.AndEqual(fieldName, fieldValue);
        }
	 
        public DbCondition Or(string fieldName, object fieldValue)
        {
            return this.OrEqual(fieldName, fieldValue);
        }
		
        public DbCondition OrderByASC(string fieldName)
        {
            sbSQL.AppendFormat(orderByAscTag, fieldName);
            return this;
        }
		
        public DbCondition OrderByDESC(string fieldName)
        {
            sbSQL.AppendFormat(orderByDescTag, fieldName);
            return this;
        }

        public DbCondition Like(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" {0} LIKE '%'+ {1} + '%' ", fieldName, paramChar + formatName);            
            return this;
        }
		
        public DbCondition AndLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" AND {0} LIKE '%'+ {1} + '%' ", fieldName, paramChar + formatName);
            return this;
        }
        public DbCondition OrLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" OR {0} LIKE '%'+ {1} + '%' ", fieldName, paramChar + formatName);
            return this;
        }
	
        public DbCondition LeftLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" {0} LIKE '%'+ {1} ", fieldName, paramChar + formatName);
            return this;
        }
		
        public DbCondition AndLeftLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" AND {0} LIKE '%'+ {1} ", fieldName, paramChar + formatName);
            return this;
        }
	
        public DbCondition OrLeftLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" OR {0} LIKE '%'+ {1}  ", fieldName, paramChar + formatName); 
            return this;
        }
		
        public DbCondition RightLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" {0} LIKE {1} + '%' ", fieldName, paramChar + formatName);
            return this;
        }
		
        public DbCondition AndRightLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" AND {0} LIKE {1} + '%' ", fieldName, paramChar + formatName);

            var aaa = sbSQL.ToString();
            return this;
        }

        public DbCondition OrRightLike(string fieldName, object fieldValue)
        {
            string formatName = FormatKey(fieldName);
            formatName = SetParameter(formatName, fieldValue);
            sbSQL.AppendFormat(" OR {0} LIKE {1} + '%' ", fieldName, paramChar + formatName);
            return this;
        }
        
        public override string ToString()
        {
            return sbSQL.ToString();
        }
		 
        private string FormatKey(string key)
        {
            int index = key.IndexOf('.');
            if (index >= 0)
                key = key.Substring(index + 1, key.Length - (index + 1));
            return key;
        }

        private string SetParameter(string key, object value)
        {             
            if (this.ContainsKey(key))
                key = Rename(key);
            base.Add(key, value);
            return key;
        }

        private void RemoveParameter(string key)
        {
            if (this.ContainsKey(key))
                this.Remove(key);
        }

        private string Rename(string key)
        {
            for (var i = 2; i <= 9999; i++)
            {               
                key = string.Format("{0}_Arg{1}",key,i.ToString());
                if (!this.ContainsKey(key))
                {
                    return key;
                }                 
            }
            return key;
        }

        #region Page Param
        public bool IsPage { get; set; }

        public int PageIndex
        {
            get
            {
                var pageIndex = 1;
                if (this.ContainsKey("PageIndex"))
                    pageIndex = this.GetInt("PageIndex");
                return pageIndex;
            }
        }

        public int PageSize
        {
            get
            {
                var pageSize = 20;
                if (this.ContainsKey("PageSize"))
                    pageSize = this.GetInt("PageSize");
                return pageSize;
            }
        }

        public int PageOffset
        {
            get
            {
                return (PageIndex - 1) * PageSize;
            }
        }

        public int StartPage
        {
            get
            {
                return PageOffset + 1;
            }
        }

        public int EndPage
        {
            get
            {
                return PageIndex * PageSize;
            }
        }

        public string OrderFields { get; set; }

        public bool IsDesc { get; set; }
              
        public void SetPageParameter(int pageIndex, int pageSize, string orderFields="Id", bool isDesc = true)
        {
            RemoveParameter("PageIndex");
            RemoveParameter("PageSize");

            this.IsPage = true;
            SetParameter("PageIndex", pageIndex);
            SetParameter("PageSize", pageSize);

            OrderFields = orderFields;
            IsDesc = isDesc;

            SetParameter("StartPage", StartPage);
            SetParameter("EndPage", EndPage);
        }


        public int GetInt(string key)
        {
            var value = this[key];
            return Convert.ToInt32(value);
        }

        public string GetString(string key)
        {
            var value = this[key];
            return Convert.ToString(value);
        }

        #endregion

        #region Base Method
        public SqlParameter[] GetParameters(DbCondition dbCondition)
        {
            var parameters = SqlUtils.CreateSqlParameters(dbCondition.Count);
            var paramEnumerator = dbCondition.GetEnumerator();
            var i = 0;
            while (paramEnumerator.MoveNext())
            {
                parameters[i].ParameterName = paramEnumerator.Key.ToString();
                parameters[i].Value = paramEnumerator.Value;
                i++;
            }
            return parameters;
        }
        #endregion
    }
}
