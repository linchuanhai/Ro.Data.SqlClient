
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Ro.Data.SqlClient
{    
    public class DataProvider
    {
        private static object locker = new object();

        private SqlTransaction _transaction = null;
        public static string ConnectionString = string.Empty;

        private static DataProvider _instance = null;
        public static DataProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataProvider();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        public DbCondition DbCondition { get; set; }
         
        #region Transaction
        public void BeginTransaction(bool openTrans=true)
        {
            if(openTrans)
                _transaction = SqlUtils.CreateSqlTransaction();
        }

        public void BeginTransaction(IsolationLevel level, bool openTrans = true)
        {
            if(openTrans)
                _transaction = SqlUtils.CreateSqlTransaction(level);
        }

        public void Commit(bool openTrans = true)
        {
            if(openTrans)
            {
                SqlUtils.Commit(_transaction);
                _transaction = null;
            }
            
        }

        public void Rollback(bool openTrans = true)
        {
            if (openTrans)
            {
                SqlUtils.Rollback(_transaction);
                _transaction = null;
            }
        }
        #endregion

        #region CRUD
        public int Insert<T>(T entity) where T :class,new()
        {      
            if (entity == null)
                return 0;

            object val = 0;      
            try
            {               
                var tableInfo = EntityUtils.GetTableModel(entity);
                var insertSqlCommand = SqlBuildUtils.BuildInsertSqlCommand(tableInfo, entity);
                val=SqlUtils.ExecuteScalar(_transaction, CommandType.Text, insertSqlCommand.CommandText, insertSqlCommand.SqlParameters);                 
                if (tableInfo.Strategy == GenerationType.Indentity)
                    ReflectionUtils.SetPropertyValue(entity, tableInfo.PrimaryKeys[0].PropertyName, val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());              
            }             
            return Convert.ToInt32(val);
        }

        public int Insert<T>(List<T> entityList,bool openTrans) where T :class,new()
        {
            if (entityList == null || entityList.Count == 0)
                return 0;

            object val = 0;          
            try
            {
                BeginTransaction(openTrans);
                foreach (T entity in entityList)
                {
                    var tableInfo = EntityUtils.GetTableModel(entity);
                    var insertSqlCommand = SqlBuildUtils.BuildInsertSqlCommand(tableInfo, entity);                                       
                    val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, insertSqlCommand.CommandText, insertSqlCommand.SqlParameters);                    
                    if(val!=null)
                    {
                        if (tableInfo.Strategy == GenerationType.Indentity)
                            ReflectionUtils.SetPropertyValue(entity, tableInfo.PrimaryKeys[0].PropertyName, val);
                    }
                    else
                    {
                        Rollback(openTrans);
                        return 0;
                    }  
                }
                Commit(openTrans);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());               
            }            
            return Convert.ToInt32(val);
        }

        public int Delete<T>(T entity) where T :class,new()
        {
            if (entity == null)
                return 0;

            object val = 0;          
            try
            {               
                var tableInfo = EntityUtils.GetTableModel(entity);
                var values = ReflectionUtils.GetPropertyValues(entity, tableInfo.PrimaryKeys);
                var deleteSqlCommand = SqlBuildUtils.BuildDeleteSqlCommand(tableInfo, values);                
                val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, deleteSqlCommand.CommandText, deleteSqlCommand.SqlParameters);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }             
            return Convert.ToInt32(val);
        }

        public int Delete<T>(ExpressionCondition expCondition) where T : class, new()
        {        
            object val = 0;
            try
            {
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);               
                var deleteSqlCommand = SqlBuildUtils.BuildDeleteSqlCommand(tableInfo, expCondition);
                val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, deleteSqlCommand.CommandText, deleteSqlCommand.SqlParameters);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            return Convert.ToInt32(val);
        }

        public int Delete<T>(List<T> entityList, bool openTrans) where T :class,new()
        {
            if (entityList == null || entityList.Count == 0)
                return 0;

            int val = 0;         
            try
            {
                BeginTransaction(openTrans);
                foreach (T entity in entityList)
                {
                    var tableInfo = EntityUtils.GetTableModel(entity);
                    var values = ReflectionUtils.GetPropertyValues(entity, tableInfo.PrimaryKeys);
                    var deleteSqlCommand = SqlBuildUtils.BuildDeleteSqlCommand(tableInfo, values);

                    val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, deleteSqlCommand.CommandText, deleteSqlCommand.SqlParameters);
                    if (val <=0)
                    {
                        Rollback(openTrans);
                        return 0;
                    }                                  
                }
                Commit(openTrans);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());              
            }
            return val;
        }
        
        public int Update<T>(T entity) where T :class,new()
        {
            if (entity == null)
                return 0;

            int val = 0;           
            try
            {                 
                var tableInfo = EntityUtils.GetTableModel(entity);
                var changeFields = EntityUtils.GetChangeColumns(entity, tableInfo);
                var updateSqlCommand = SqlBuildUtils.BuildUpdateSqlcommand(tableInfo, changeFields, entity);       
                if(updateSqlCommand!=null)      
                    val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, updateSqlCommand.CommandText, updateSqlCommand.SqlParameters);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            return val;
        }

        public int Update<T>(List<T> entityList, bool openTrans) where T :class,new()
        {
            if (entityList == null || entityList.Count == 0)
                return 0;

            int val = 0;          
            try
            {
                BeginTransaction(openTrans);
                foreach (T entity in entityList)
                {
                    var tableInfo = EntityUtils.GetTableModel(entity);
                    var changeFields = EntityUtils.GetChangeColumns(entity, tableInfo);
                    var updateSqlCommand = SqlBuildUtils.BuildUpdateSqlcommand(tableInfo, changeFields, entity);

                    if (updateSqlCommand != null)
                        val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, updateSqlCommand.CommandText, updateSqlCommand.SqlParameters);
                    if (val <=0)
                    {
                        Rollback(openTrans);
                        return 0;
                    }
                }
                Commit(openTrans);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());               
            }            
            return Convert.ToInt32(val);
        }

        public int ExecuteNonQuery(string sql)
        {
            int val = 0;            
            try
            {
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                val =SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, sql);           
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            return val;
        }

        public int ExecuteNonQuery(string sql, ParamMap param)
        {
            int val = 0;            
            try
            {
                var parameters = param.GetParameters(param);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));


                val = SqlUtils.ExecuteNonQuery(_transaction, CommandType.Text, sql, parameters);              
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }            
            return val;
        }

        public int Count(string sql)
        {
            int count = 0;         
            try
            {
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql);
                count = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                 
            }           
            return count;
        }
         
        public int Count(string sql, ParamMap param)
        {
            int count = 0;          
            try
            {
                var parameters = param.GetParameters(param);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql, parameters);
                count = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                 
            }            
            return count;
        }
         
        public int Count<T>(bool queryLock = true) where T :class,new()
        {
            int count = 0;          
            try
            {               
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildFindCountSql(tableInfo, queryLock);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql);
                count = Convert.ToInt32(val);                
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                 
            }            
            return count;
        }

        public int Count<T>(DbCondition dbCondition, bool queryLock = true) where T :class,new()
        {
            int count = 0;          
            try
            {               
                T entity = new T();               

                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildFindCountSql(tableInfo, dbCondition, queryLock);
                var parameters = dbCondition.GetParameters(dbCondition);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql, parameters);
                count = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }          
            return count;
        }
         
        public int Count<T>(ExpressionCondition expCondition, bool queryLock = true) where T : class, new()
        {
            int count = 0;
            try
            {
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildFindCountSql(tableInfo, expCondition, queryLock);
                 
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql);
                count = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            return count;

        }

        public int Count<T>(string sql,DbCondition dbCondition) where T :class,new()
        {
            int count = 0;           
            try
            {  
                sql = SqlBuildUtils.BuildFindCountSql(sql, dbCondition);
                var parameters = dbCondition.GetParameters(dbCondition);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                var val = SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql, parameters);
                count = Convert.ToInt32(val);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());               
            }
            return count;
        }

        public object ExecuteScalar(string sql)
        {
            try
            {
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));
                return SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            return null;
        }

        public object ExecuteScalar(string sql, ParamMap param)
        {
            try
            {
                var parameters = param.GetParameters(param);
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));
                return SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql, parameters);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            return null;
        }

        public object ExecuteScalar(string sql, DbCondition dbCondition)
        {
            try
            {
                sql += dbCondition.ToString();
                var parameters = dbCondition.GetParameters(dbCondition);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                return SqlUtils.ExecuteScalar(_transaction, CommandType.Text, sql, parameters);

            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            return null;
        }


        public PageResult<T> FindPage<T>(DbCondition dbCondition, bool queryLock = true) where T :class,new()
        {
            PageResult<T> pageResult = new PageResult<T>();
            List<T> list = new List<T>();
            SqlDataReader reader = null;          
            try
            {              
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var selectSqlCommand = SqlBuildUtils.BuildSelectSqlCommand(tableInfo, queryLock);
                var count = Count<T>(dbCondition);
                var parameters = dbCondition.GetParameters(dbCondition);
                 
                var sql = selectSqlCommand.CommandText;
                sql += dbCondition.ToString();

                if (dbCondition.IsPage)
                    sql = SqlBuildUtils.BuildRowNumberPageSql(sql, dbCondition.OrderFields, dbCondition.IsDesc);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql, parameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);                 

                pageResult.TotalCount = count;
                pageResult.DataSource = list;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return pageResult;
        }

        public PageResult<T> FindPage<T>(int pageIndex,int PageSize,ExpressionCondition expCondition, bool queryLock = true) where T : class, new()
        {
            PageResult<T> pageResult = new PageResult<T>();
            List<T> list = new List<T>();
            SqlDataReader reader = null;
            try
            {
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var selectSqlCommand = SqlBuildUtils.BuildSelectSqlCommand(tableInfo, queryLock);
                var count = Count<T>(expCondition);         

                var sql = selectSqlCommand.CommandText;
                sql += expCondition.ToString();

                sql = SqlBuildUtils.BuildRowNumberPageSql(sql,pageIndex,PageSize,expCondition.OrderByFields);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql);
                list = EntityUtils.ToList<T>(reader, tableInfo);

                pageResult.TotalCount = count;
                pageResult.DataSource = list;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return pageResult;
        }

        public PageResult<T> FindPage<T>(ParamMap param, bool queryLock = true) where T :class,new()
        {
            T entity = new T();
            var tableInfo = EntityUtils.GetTableModel(entity);
            var selectSqlCommand = SqlBuildUtils.BuildSelectSqlCommand(tableInfo,queryLock);
            var sql = selectSqlCommand.CommandText;            
            return FindPage<T>(sql, param);
        }

        public PageResult<T> FindPage<T>(string sql, ParamMap param) where T :class,new()
        {
            PageResult<T> pageResult = new PageResult<T>();
            List<T> list = new List<T>();
            SqlDataReader reader = null;           
            try
            {             
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);

                sql = sql.ToLower().Trim();
                var countSql = SqlBuildUtils.BuildCountSql(sql);
                var count = Count(countSql, param);
                var parameters = param.GetParameters(param);

                if (param.IsPage && !SqlBuildUtils.IsPageQuery(sql))
                    sql = SqlBuildUtils.BuildRowNumberPageSql(sql, param.OrderFields, param.IsDesc);   
                   

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql, parameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);
                
                pageResult.TotalCount = count;
                pageResult.DataSource = list;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());               
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return pageResult;
        }
         
        public T Get<T>(object[] primaryKeys, bool queryLock = true) where T : class, new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;
            try
            {
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                if (tableInfo.PrimaryKeys.Count != primaryKeys.Length)
                    throw new ArgumentException("条件参数与表主键个数一致！");


                var selectSqlCommand = SqlBuildUtils.BuildSelectSqlCommand(tableInfo, primaryKeys, queryLock);

                reader = SqlUtils.ExecuteReader(CommandType.Text, selectSqlCommand.CommandText,
                    selectSqlCommand.SqlParameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list.FirstOrDefault();
        }

        public T Get<T>(DbCondition dbCondition, bool queryLock = true) where T :class,new()
        {
            var list = Find<T>(dbCondition, queryLock);
            return list.FirstOrDefault();
        }

        public T Get<T>(ExpressionCondition expCondition, bool queryLock = true) where T : class, new()
        {
            var list = Find<T>(expCondition);
            return list.FirstOrDefault();
        }

        public List<T> GetAll<T>(bool queryLock = true) where T :class,new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;          
            try
            {               
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var selectSqlCommand = SqlBuildUtils.BuildSelectSqlCommand(tableInfo);

                reader = SqlUtils.ExecuteReader(CommandType.Text, selectSqlCommand.CommandText,
                    selectSqlCommand.SqlParameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);                
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }

        public List<T> Find<T>(string sql) where T :class,new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;             
            try
            {               
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql);              
                list = EntityUtils.ToList<T>(reader, tableInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());               
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return list;
        }

        public List<T> Find<T>(string sql, ParamMap param) where T :class,new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;           
            try
            {      
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var parameters = param.GetParameters(param);
                 
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql, parameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);              
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());             
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }

        public List<T> Find<T>(DbCondition dbCondition, bool queryLock = true) where T :class,new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;             
            try
            {               
                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildFindSql(tableInfo, dbCondition, queryLock);
                var parameters = dbCondition.GetParameters(dbCondition);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql, parameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);                
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                 
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }

        public List<T> Find<T>(ExpressionCondition expCondition, bool queryLock = true) where T : class, new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;
            try
            {

                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildFindSql(tableInfo, expCondition, queryLock);
             
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql);
                list = EntityUtils.ToList<T>(reader, tableInfo);

            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }


        public List<T> Top<T>(int count,DbCondition dbCondition, bool queryLock = true) where T :class,new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;
            try
            {

                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildTopSql(count,tableInfo, dbCondition, queryLock);
                var parameters = dbCondition.GetParameters(dbCondition);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql, parameters);
                list = EntityUtils.ToList<T>(reader, tableInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());                
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }

        public List<T> Top<T>(int count, ExpressionCondition expCondition, bool queryLock = true) where T : class, new()
        {
            List<T> list = new List<T>();
            SqlDataReader reader = null;
            try
            {

                T entity = new T();
                var tableInfo = EntityUtils.GetTableModel(entity);
                var sql = SqlBuildUtils.BuildTopSql(count, tableInfo, expCondition, queryLock);
              
                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                reader = SqlUtils.ExecuteReader(CommandType.Text, sql);
                list = EntityUtils.ToList<T>(reader, tableInfo);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return list;
        }

        #endregion

        #region Join Query
        public List<T> JoinQuery<T>(string sql) where T :class,new()
        {
            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

            var dataset = SqlUtils.ExecuteDataSet(CommandType.Text, sql);
            return EntityUtils.ToList<T>(dataset);            
        }

        public List<T> JoinQuery<T>(string sql, ParamMap param) where T :class,new()
        {
            var parameters = param.GetParameters(param);
            
            if (SqlClientConst.RoDataSqlClientDebug)
                LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

            var dataset = SqlUtils.ExecuteDataSet(CommandType.Text, sql, parameters);
            return EntityUtils.ToList<T>(dataset);
        }

        public PageResult<T> JoinQueryPage<T>(string sql, DbCondition dbCondition) where T :class,new()
        {             
            PageResult<T> pageResult = new PageResult<T>();
            List<T> list = new List<T>();    
            try
            {               
                var count = Count<T>(sql, dbCondition);
                var parameters = dbCondition.GetParameters(dbCondition);
               
                sql += dbCondition.ToString();
                if (dbCondition.IsPage && !SqlBuildUtils.IsPageQuery(sql))
                    sql = SqlBuildUtils.BuildRowNumberPageSql(sql, dbCondition.OrderFields, dbCondition.IsDesc);                

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql, parameters));

                var dataset = SqlUtils.ExecuteDataSet(CommandType.Text, sql, parameters);
                list= EntityUtils.ToList<T>(dataset);
                
                pageResult.TotalCount = count;
                pageResult.DataSource = list;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());              
            }
           
            return pageResult;
        }

        public PageResult<T> JoinQueryPage<T>(string sql, int pageIndex, int PageSize, ExpressionCondition expCondition) where T : class, new()
        {
            PageResult<T> pageResult = new PageResult<T>();
            List<T> list = new List<T>();
            try
            {
                var count = Count<T>(expCondition);
             
                sql += expCondition.ToString();
                sql = SqlBuildUtils.BuildRowNumberPageSql(sql, pageIndex,PageSize, expCondition.OrderByFields);

                if (SqlClientConst.RoDataSqlClientDebug)
                    LogHelper.WriteLog(SqlUtils.ParmsToString(sql));

                var dataset = SqlUtils.ExecuteDataSet(CommandType.Text, sql);
                list = EntityUtils.ToList<T>(dataset);

                pageResult.TotalCount = count;
                pageResult.DataSource = list;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.ToString());
            }

            return pageResult;
        }

        #endregion

        #region Private method
        private void Init()
        {
         
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException("ConnectionString 未设置!");

            SqlUtils.ConnectionString = ConnectionString;           
            DbCondition = new DbCondition();
        }       
        #endregion
    }
}
