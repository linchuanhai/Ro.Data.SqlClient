using System.Data.SqlClient;

namespace Ro.Data.SqlClient
{
   public class DbCommand
    {
       public string CommandText { get; set; }

       public SqlParameter[] SqlParameters { get; set; }         
    }
}
