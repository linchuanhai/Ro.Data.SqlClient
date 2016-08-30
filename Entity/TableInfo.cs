using System;
using System.Collections.Generic;

namespace Ro.Data.SqlClient
{
    public class TableInfo:ICloneable
    {
        public string TableName { get; set; }
        
        public List<PrimaryKeyInfo> PrimaryKeys { get; set; }

        /// <summary>
        /// 主键生成方式,参考GenerationType定义
        /// </summary>
        public int Strategy { get; set; }

        public List<ColumnInfo> Columns { get; set; }            

        public object Clone()
        {
            var tableInfo = new TableInfo();
            tableInfo.TableName = TableName;
            tableInfo.PrimaryKeys = PrimaryKeys;
            tableInfo.Strategy = Strategy;
            tableInfo.Columns = Columns;
            return tableInfo;
        }
    }
}
