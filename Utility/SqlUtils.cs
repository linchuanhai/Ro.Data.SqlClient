using System.Text;
using System.Data;
using System.Data.SqlClient;
using System;


namespace Ro.Data.SqlClient
{     
    public class SqlUtils
    {
        #region SqlConnection
        public static int CommandTimeout = 30;
        public static string ConnectionString = string.Empty;
       
        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlConnection GetSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public static SqlConnection GetSqlConnection(SqlTransaction trans)
        {
            if (trans != null&& trans.Connection!=null)
                return trans.Connection;
            return GetSqlConnection();
        }
        #endregion

        #region Transaction
        public static SqlTransaction CreateSqlTransaction()
        {
            var connection = GetSqlConnection();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return connection.BeginTransaction();
        }

        public static SqlTransaction CreateSqlTransaction(IsolationLevel level)
        {
            var connection = GetSqlConnection();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return connection.BeginTransaction(level);
        }

        public static void Commit(SqlTransaction trans)
        {
            if (trans != null && trans.Connection != null)
            {
                if (trans.Connection.State != ConnectionState.Closed)
                {
                    trans.Commit();  
                }               
            }
        }

        public static void Rollback(SqlTransaction trans)
        {
            if (trans != null && trans.Connection != null)
            {
                if (trans.Connection.State != ConnectionState.Closed)
                {
                    trans.Rollback();   
                }
            }
        }
        #endregion        

        #region ExecuteNonQuery
        public static int ExecuteNonQuery(string storedProcedure, params SqlParameter[] commandParameters)
        {
            var val = -1;
            SqlConnection connection = null;
            try
            {
                var command = new SqlCommand();
                connection = GetSqlConnection();
                PrepareCommand(command, connection, null, CommandType.StoredProcedure, storedProcedure, commandParameters);
                val = command.ExecuteNonQuery();
                command.Parameters.Clear();
                connection.Close();               
            }
            catch(Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val;             
        }

        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            var val = -1;
            SqlConnection connection = null;
            try
            {
                var command = new SqlCommand();
                connection = GetSqlConnection();
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                val = command.ExecuteNonQuery();
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val;
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            var val = -1;         
            try
            {
                var command = new SqlCommand();              
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                val = command.ExecuteNonQuery();
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val; 
        }

        public static int ExecuteNonQuery(SqlTransaction trans, string storedProcedure, params SqlParameter[] commandParameters)
        {
            var val = -1;
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection(trans);
                var command = new SqlCommand();
                PrepareCommand(command, connection, trans, CommandType.StoredProcedure, storedProcedure, commandParameters);
                val = command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                trans = null;
                if (connection != null)
                    connection.Close();
                
            }
            finally
            {
                if (trans == null && connection!=null&& connection.State != ConnectionState.Closed)
                    connection.Close();
            }
            return val; 
        }

        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            var val = -1;
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection(trans);
                var command = new SqlCommand();
                PrepareCommand(command, connection, trans, cmdType, cmdText, commandParameters);
                val = command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                trans = null;
                if (connection != null)
                    connection.Close();                
            }
            finally
            {
                if (trans == null && connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
            return val;
        }

        #endregion
         
        #region ExecuteScalar
        public static object ExecuteScalar(string storedProcedure, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            object val = null;
            try
            {
                connection = GetSqlConnection();
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, CommandType.StoredProcedure, storedProcedure, commandParameters);
                val = cmd.ExecuteScalar();
                if (val == DBNull.Value)
                    val = null;
                cmd.Parameters.Clear();
                connection.Close();             
            }
            catch(Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val;               
        }

        public static object ExecuteScalar(CommandType cmdType, string cmdText, SqlTransaction transaction, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            object val = null;
            try
            {
                connection = GetSqlConnection();
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
                if (val == DBNull.Value)
                    val = null;
                cmd.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val;
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {          
            object val = null;
            try
            {               
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
                if (val == DBNull.Value)
                    val = null;
                cmd.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return val;
        }

        public static object ExecuteScalar(SqlTransaction trans, string storedProcedure, params SqlParameter[] commandParameters)
        {
            object val = null;
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection(trans);
                var cmd = new SqlCommand();                
                PrepareCommand(cmd, connection, trans, CommandType.StoredProcedure, storedProcedure, commandParameters);
                val = cmd.ExecuteScalar();
                if (val == DBNull.Value)
                    val = null;
                cmd.Parameters.Clear();              
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                trans = null;
                if (connection != null)
                    connection.Close();                
            }
            finally
            {
                if (trans == null && connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
            return val;            
        }

        public static object ExecuteScalar(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            object val = null;
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection(trans);
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, trans, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
                if (val == DBNull.Value)
                    val = null;
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                trans = null;
                if (connection != null)
                    connection.Close();       
                         
            }
            finally
            {
                if (trans == null && connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
            return val;
        }
        #endregion

        #region ExecuteReader
        public static SqlDataReader ExecuteReader(string storedProcedure, params SqlParameter[] commandParameters)
        {      
            SqlConnection connection = null;
            SqlDataReader reader = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, CommandType.StoredProcedure, storedProcedure, commandParameters);
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();             
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                if (connection!=null)
                    connection.Close();
                
            }
            return reader;
        }

        public static SqlDataReader ExecuteReader(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            SqlDataReader reader = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return reader;
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {             
            SqlDataReader reader = null;
            try
            {               
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return reader;           
        }

        #endregion

        #region ExecuteDataSet
        public static DataSet ExecuteDataSet(string storedProcedure, params SqlParameter[] commandParameters)
        {
            DataSet dataSet = new DataSet();
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, CommandType.StoredProcedure, storedProcedure, commandParameters);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                if (connection != null)
                    connection.Close();
            }
            return dataSet;
        }

        public static DataSet ExecuteDataSet(CommandType commandType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataSet dataSet = new DataSet();
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, commandType, cmdText, commandParameters);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();

            }
            return dataSet;
        }

        public static DataSet ExecuteDataSet(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataSet dataSet = new DataSet();
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();

            }
            return dataSet;
        }

        #endregion        

        #region ExecuteTable
        public static DataTable ExecuteTable(string storedProcedure, params SqlParameter[] commandParameters)
        {
            DataTable table = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, CommandType.StoredProcedure, storedProcedure, commandParameters);
                var adapter= new SqlDataAdapter(command);                
                adapter.Fill(table);
                command.Parameters.Clear();
                connection.Close();
            }
            catch(Exception ex)
            {
                LogHelper.Write(ParmsToString(storedProcedure, ex, commandParameters));
                if (connection!=null)
                    connection.Close();
                
            } 
            return table;
        }

        public static DataTable ExecuteTable( CommandType commandType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataTable table = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, commandType, cmdText, commandParameters);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return table;
        }

        public static DataTable ExecuteTable(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataTable table = new DataTable();          
            try
            {
                connection = GetSqlConnection();
                var command = new SqlCommand();
                PrepareCommand(command, connection, null, cmdType, cmdText, commandParameters);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
                command.Parameters.Clear();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write(ParmsToString(cmdText, ex, commandParameters));
                if (connection != null)
                    connection.Close();
                
            }
            return table;
        }

        #endregion        
 
        #region SqlParameter      
        public static SqlParameter CreateSqlParameter(string paramName, object value)
        {
            var param = new SqlParameter();         
            param.ParameterName = paramName;
            param.Value = value??DBNull.Value;
            return param;
        }
  
        public static SqlParameter CreateDbOutParameter(string paramName, int size)
        {
            var param = new SqlParameter();
            param.Direction = ParameterDirection.Output;
            param.ParameterName = paramName;
            param.Size = size;
            return param;
        }
         
        public static SqlParameter CreateSqlParameter(string paramName, object value, ParameterDirection direction)
        {
            var param = new SqlParameter();
            param.Direction = direction;
            param.ParameterName = paramName;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        public static SqlParameter CreateSqlParameter(string paramName, object value, int size, ParameterDirection direction)
        {
            var param = new SqlParameter();
            param.Direction = direction;
            param.ParameterName = paramName;
            param.Value = value ?? DBNull.Value;
            param.Size = size;
            return param;
        }

        public static SqlParameter CreateSqlParameter(string paramName, object value, DbType dbType,int size, ParameterDirection direction)
        {
            var param = new SqlParameter();
            param.Direction = direction;
            param.DbType = dbType;
            param.ParameterName = paramName;
            param.Value = value ?? DBNull.Value;
            param.Size = size;
            return param;
        }

        public static SqlParameter[] CreateSqlParameters(int size)
        {          
            int i = 0;
            SqlParameter[] param = null;
            param = new SqlParameter[size];
            while (i < size)
            {
                param[i] = new SqlParameter();
                i++;
            }
            return param;
        }
        #endregion
        
        #region 特定SQL 操作       
        public static int TruncateTable(string tableName)
        {
            var cmdText = string.Format("Truncate Table {0}", tableName);
            return ExecuteNonQuery(CommandType.Text, cmdText);
        }

        public static string GetSafeSql(string input)
        {
            string safeString = input.Replace("'", "''");
            safeString = safeString.Replace(";", "");
            safeString = safeString.Replace("%", "");
            safeString = safeString.Replace("#", "");
            safeString = safeString.Replace("&", "");
            safeString = safeString.Replace("|", "");
            safeString = safeString.Replace("\"", "");
            safeString = safeString.Replace("=", "");
            safeString = safeString.Replace("(", "");
            safeString = safeString.Replace(")", "");
            return safeString;
        }

        public static string ParmsToString(string cmdText, params SqlParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CmdText : {0}", cmdText);
            sb.AppendLine("");
            sb.AppendLine("Parms : ");

            if(parms!=null)
            {
                foreach (SqlParameter parm in parms)
                {
                    sb.AppendFormat("{0} = {1}", parm.ParameterName, parm.Value);
                    sb.AppendLine("");
                }
            }
            
            return sb.ToString();
        }

        public static string ParmsToString(string cmdText,Exception ex, params SqlParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CmdText : {0}", cmdText);
            sb.AppendLine("");
            sb.AppendLine("Parms : ");

            if (parms != null)
            {
                foreach (SqlParameter parm in parms)
                {
                    sb.AppendFormat("{0} = {1}", parm.ParameterName, parm.Value);
                    sb.AppendLine("");
                }
            }
             sb.AppendLine("");
             sb.AppendLine(string.Format("Exception:"+ex.ToString()));
            return sb.ToString();
        }
        #endregion

        #region 私有方法 
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;
            cmd.CommandTimeout = CommandTimeout;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        #endregion
    }
}