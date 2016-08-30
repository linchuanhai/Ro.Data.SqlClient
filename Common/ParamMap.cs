using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Ro.Data.SqlClient
{
    public class ParamMap : Hashtable
    {
        public static ParamMap NewMap()
        {
            var map=new ParamMap();           
            return map;
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

        public void SetPageParameter(int pageIndex, int pageSize, string orderFields,bool isDesc=true)
        {
            this.IsPage = true;
            SetParameter("PageIndex", pageIndex);
            SetParameter("PageSize", pageSize);

            OrderFields = orderFields;
            IsDesc = IsDesc;

            SetParameter("StartPage", StartPage);
            SetParameter("EndPage", EndPage);

            RemoveParameter("PageIndex");
            RemoveParameter("PageSize");
        }
       
        #endregion

        #region Base Method
        public void SetParameter(string key, object value)
        {
            if (this.ContainsKey(key))
                this.Remove(key);
            base.Add(key, value);
        }

        public void RemoveParameter(string key)
        {
            if (this.ContainsKey(key))
                this.Remove(key);
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

        public SqlParameter[] GetParameters(ParamMap paramMap)
        {
            var parameters = SqlUtils.CreateSqlParameters(paramMap.Count);
            var paramEnumerator = paramMap.GetEnumerator();
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
