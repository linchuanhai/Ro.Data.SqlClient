using System.Configuration;

namespace Ro.Data.SqlClient
{
    public class SqlClientConst
    {
        #region Ro.Data.SqlClient
        public static bool RoDataSqlClientDebug
        {
            get
            {
                var debug = ConfigurationManager.AppSettings["Ro.Data.SqlClient.Debug"] ?? string.Empty;
                return string.Compare(debug, "true", true) == 0;                
            }
        }
        #endregion
    }
}
