
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace Ro.Data.SqlClient
{
    public class SqlBuildUtils
    {
        #region Common Sql
        private static string queryIdentitySql = "SELECT SCOPE_IDENTITY() as AutoId ";
        private static string queryRowNumberSqlPage = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS RowNumber, {1}) AS tmp_tbl WHERE RowNumber BETWEEN @StartPage and @EndPage ";
        private static string queryRowNumberSqlPageNoParam = "SELECT * FROM (SELECT ROW_NUMBER() OVER({0}) AS RowNumber, {1}) AS tmp_tbl WHERE RowNumber BETWEEN {2} and {3} ";


        #endregion

        #region Build DbCommand     
        public static DbCommand BuildInsertSqlCommand(TableInfo tableInfo, object entity)
        {          
            var sbColumns = new StringBuilder();
            var sbValues = new StringBuilder();

            var insertColumns = new List<ColumnInfo>();
            if (tableInfo.Columns != null && tableInfo.Columns.Count > 0)
            {
                var flag = false;
                foreach (var column in tableInfo.Columns)
                {
                    if (column.IsPrimaryKey && tableInfo.Strategy == GenerationType.Indentity)
                        continue;

                    if (column.Ignore || !column.IsInsert)
                        continue;

                    if (column.IsPrimaryKey
                        && (tableInfo.Strategy == GenerationType.Guid))
                    {
                        column.Value = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        column.Value = ReflectionUtils.GetPropertyValue(entity, column.PropertyName);
                    }

                    if (flag)
                    {
                        sbColumns.Append(",");
                        sbValues.Append(",");
                    }
                    flag = true;                  
                    sbColumns.AppendFormat("[{0}]", column.ColumnName);
                    sbValues.AppendFormat("@{0}", column.ColumnName);                 
                    insertColumns.Add(column);
                }
            }
            var sql = "INSERT INTO [{0}]({1}) VALUES({2})";
            sql = string.Format(sql, tableInfo.TableName, sbColumns.ToString(), sbValues.ToString());

            if (tableInfo.Strategy == GenerationType.Indentity&& tableInfo.PrimaryKeys.Count==1)            
                sql = sql + queryIdentitySql;            
                 
            var command = new DbCommand();
            command.CommandText = sql;
            command.SqlParameters = GetParameters(insertColumns);


            if(SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, command.SqlParameters));

            return command;
        }

        public static DbCommand BuildDeleteSqlCommand(TableInfo tableInfo, object[] values)
        {
            var command = new DbCommand();        
            string sql = "DELETE FROM [{0}] WHERE 1=1";
            sql = string.Format(sql, tableInfo.TableName);
           
            var parameters = SqlUtils.CreateSqlParameters(tableInfo.PrimaryKeys.Count);
            for (var i = 0; i < tableInfo.PrimaryKeys.Count; i++)
            {
                sql = sql + string.Format(" AND [{0}]=@{0} ", tableInfo.PrimaryKeys[i].ColumnName);
                parameters[i].ParameterName = tableInfo.PrimaryKeys[i].ColumnName;
                parameters[i].Value = values[i];
            }
            command.CommandText = sql;
            command.SqlParameters = parameters;

            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, command.SqlParameters));

            return command;
        }

        public static DbCommand BuildDeleteSqlCommand(TableInfo tableInfo, ExpressionCondition expCondition)
        {
            var command = new DbCommand();
            string sql = "DELETE FROM [{0}] ";
            sql = string.Format(sql, tableInfo.TableName);
            sql += expCondition.ToString();

            command.CommandText = sql;
            command.SqlParameters = null;

            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

            return command;
        }

        public static DbCommand BuildUpdateSqlcommand(TableInfo tableInfo,Dictionary<string,object> changeFields, object entity)
        {           
            var sbBody = new StringBuilder();
            var updateColumns = new List<ColumnInfo>();
            
            if (tableInfo.Columns != null && tableInfo.Columns.Count > 0)
            {
                var flag = false;
                foreach (var column in tableInfo.Columns)
                {
                    if (column.Ignore || !column.IsUpdate)
                        continue;

                    if (!column.IsPrimaryKey && changeFields!=null&& changeFields.Count>0
                        && !changeFields.ContainsKey(column.ColumnName))
                        continue;

                    column.Value = ReflectionUtils.GetPropertyValue(entity, column.PropertyName);

                    if (column.Value == null)
                        continue;

                    if (!column.IsPrimaryKey)
                    {
                        if (flag)
                            sbBody.Append(",");
                        flag = true;
                        sbBody.AppendFormat("[{0}]=@{0}",column.ColumnName);                       
                    }
                    updateColumns.Add(column);
                }
            }
            string sql = "UPDATE [{0}] SET {1} WHERE 1=1";
            sql = string.Format(sql, tableInfo.TableName, sbBody.ToString());

            for (var i = 0; i < tableInfo.PrimaryKeys.Count; i++)
            {
                sql = sql + string.Format(" AND [{0}]=@{0} ", tableInfo.PrimaryKeys[i].ColumnName);                
            }

            var command = new DbCommand();
            command.CommandText = sql;
            command.SqlParameters = GetParameters(updateColumns);

            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, command.SqlParameters));

            if (string.IsNullOrEmpty(sbBody.ToString()))
                return null;

            return command;
        } 
            
        public static DbCommand BuildSelectSqlCommand(TableInfo tableInfo, bool queryLock = true)
        {                    
            var sbColumns = new StringBuilder();
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                sbColumns.AppendFormat("[{0}]",column.ColumnName);
            }

            string sql = "SELECT {0} FROM [{1}] ";
            sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";


            var command = new DbCommand();
            command.CommandText = sql;
            command.SqlParameters = null;

            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, command.SqlParameters));

            return command;
        }
         
        public static DbCommand BuildSelectSqlCommand(TableInfo tableInfo, object[] values, bool queryLock = true)
        {                     
            var sbColumns = new StringBuilder();
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                sbColumns.AppendFormat("[{0}]", column.ColumnName);               
            }

            string sql = "SELECT {0} FROM [{1}] ";
             
            sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";
            sql += " WHERE 1=1 ";

            var command = new DbCommand();
            var parameters = SqlUtils.CreateSqlParameters(tableInfo.PrimaryKeys.Count);
            for(var i=0;i< tableInfo.PrimaryKeys.Count;i++)           
            {
                sql = sql + string.Format(" AND [{0}]=@{0} ", tableInfo.PrimaryKeys[i].ColumnName);
                parameters[i].ParameterName = tableInfo.PrimaryKeys[i].ColumnName;
                parameters[i].Value = values[i];
            }
            command.CommandText = sql;       
            command.SqlParameters = parameters;

            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, command.SqlParameters));

            return command;
        }
        #endregion
        
        #region Build Sql        
        public static bool IsPageQuery(string sql)
        {          
            var regex = new Regex(@"select\s+top\s+\d+\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (sql.ToUpper().IndexOf("ROW_NUMBER()") == -1 && regex.Matches(sql).Count < 2)
                return false;
            return true;
        }

        public static string BuildCountSql(string sql)
        {
            sql = sql.TrimStart();
            var index = sql.ToUpper().IndexOf("FROM");
            var otherSql = sql.Substring(index);
            sql= "SELECT COUNT(*) " + otherSql;
            return sql;
        }
        
        public static string BuildRowNumberPageSql(string sql,int pageIndex,int pageSize, string orderFields)
        {
            sql = sql.TrimStart();
            if (string.IsNullOrEmpty(orderFields))
                throw new ArgumentException("构建分页查询需要显示声明Order By字段!");
               
            var pageBody = sql.Substring(6);        

            var startPage = (pageIndex - 1) * pageSize + 1;
            var endPage = pageIndex * pageSize;

            sql = string.Format(queryRowNumberSqlPageNoParam, orderFields, pageBody, startPage, endPage);
            return sql;
        }


        public static string BuildRowNumberPageSql(string sql, string orderFields, bool desc)
        {
            sql = sql.TrimStart();
            if (string.IsNullOrEmpty(orderFields))
                throw new ArgumentException("构建分页查询需要显示声明Order By字段!");
          
            var orderBy = orderFields + (desc ? " DESC " : " ASC ");
            var pageBody = sql.Substring(6);
            sql = string.Format(queryRowNumberSqlPage, orderBy, pageBody);
            return sql;
        }
       
        public static string BuildFindSql(TableInfo tableInfo, DbCondition condition, bool queryLock = true)
        {  
            StringBuilder sbColumns = new StringBuilder();

            var columns = tableInfo.Columns;
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                string key = column.ColumnName.Trim();
                sbColumns.Append(key);
            }         

            string sql = string.Empty;
            if (string.IsNullOrEmpty(condition.queryString))
            {
                sql = "SELECT {0} FROM [{1}]";
                sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

                if (!queryLock)
                    sql += " WITH (NOLOCK) ";
                sql += condition.ToString();
            }
            else
            {
                sql = condition.ToString();
            }        
            return sql;
        }

        public static string BuildFindSql(TableInfo tableInfo, ExpressionCondition condition, bool queryLock = true)
        {
            StringBuilder sbColumns = new StringBuilder();
            var columns = tableInfo.Columns;
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                string key = column.ColumnName.Trim();
                sbColumns.Append(key);
            }

            string sql ="SELECT {0} FROM [{1}]";
            sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            sql += condition.ToString();
            return sql;
        }

        public static string BuildFindSql(TableInfo tableInfo, string where, bool queryLock = true)
        {
            StringBuilder sbColumns = new StringBuilder();

            var columns = tableInfo.Columns;
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                string key = column.ColumnName.Trim();
                sbColumns.Append(key);
            }

            string sql = string.Empty;
            sql = "SELECT {0} FROM [{1}]";
            sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            if (!string.IsNullOrEmpty(where))
            {
                sql += " where 1=1 AND " + where;
            }             
            return sql;
        }


        public static string BuildFindCountSql(TableInfo tableInfo, bool queryLock = true)
        {
            string sql = "SELECT COUNT(0) FROM [{0}]";   
            sql = string.Format(sql, tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";
            return sql;
        }    

        public static string BuildFindCountSql(TableInfo tableInfo, DbCondition condition, bool queryLock = true)
        {
            string sql = "SELECT COUNT(0) FROM [{0}]";
            sql = string.Format(sql, tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            sql += condition.ToString();
            return sql;
        }

        public static string BuildFindCountSql(TableInfo tableInfo, ExpressionCondition expCondition, bool queryLock = true)
        {
            string sql = "SELECT COUNT(0) FROM [{0}]";
            sql = string.Format(sql, tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            sql += expCondition.ToString();           
            return sql;
        }

        public static string BuildFindCountSql(string sql, DbCondition condition)
        {
            sql = BuildCountSql(sql);         
            sql += condition.ToString();
            return sql;
        }

        public static string BuildFindCountSql(TableInfo tableInfo, string where, bool queryLock = true)
        {
            string sql = "SELECT COUNT(0) FROM [{0}]";
            sql = string.Format(sql, tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            if (!string.IsNullOrEmpty(where))
            {
                sql +=" WHERE 1=1 AND "+ where;
            }            
            return sql;
        }

        public static string BuildTopSql(int count,TableInfo tableInfo, DbCondition condition, bool queryLock = true)
        {
            StringBuilder sbColumns = new StringBuilder();

            var columns = tableInfo.Columns;
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                string key = column.ColumnName.Trim();
                sbColumns.Append(key);
            }

            string sql = string.Empty;
            if (string.IsNullOrEmpty(condition.queryString))
            {
                sql ="SELECT TOP "+count.ToString()+" {0} FROM [{1}]";
                sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

                if (!queryLock)
                    sql += " WITH (NOLOCK) ";

                sql += condition.ToString();
            }
            else
            {
                sql = condition.ToString();
            }
            return sql;
        }

        public static string BuildTopSql(int count, TableInfo tableInfo, ExpressionCondition condition, bool queryLock = true)
        {
            StringBuilder sbColumns = new StringBuilder();

            var columns = tableInfo.Columns;
            var flag = false;
            foreach (var column in tableInfo.Columns)
            {
                if (flag)
                    sbColumns.Append(",");
                flag = true;
                string key = column.ColumnName.Trim();
                sbColumns.Append(key);
            }

            string sql = "SELECT TOP " + count.ToString() + " {0} FROM [{1}]";
            sql = string.Format(sql, sbColumns.ToString(), tableInfo.TableName);

            if (!queryLock)
                sql += " WITH (NOLOCK) ";

            sql += condition.ToString();
            return sql;
        }
        #endregion       

        #region Parameters
        private static SqlParameter[] GetParameters(List<ColumnInfo> columns)
        {
            var parameters = SqlUtils.CreateSqlParameters(columns.Count);
            for (var i = 0; i < columns.Count; i++)
            {                
                parameters[i] = SqlUtils.CreateSqlParameter(columns[i].ColumnName, columns[i].Value);
            }
            return parameters;
        }   
        #endregion
    }
}
