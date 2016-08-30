using System;

namespace Ro.Data.SqlClient
{
    public class PrimaryKeyInfo
    {
        public PrimaryKeyInfo()
        {
        } 

        public string PropertyName { get; set; }

        public string ColumnName { get; set; }

        public object Value { get; set; }

        public string TypeName { get; set; }       
    }
}
