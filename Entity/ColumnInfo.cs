using System;

namespace Ro.Data.SqlClient
{
    public class ColumnInfo
    {
        public ColumnInfo()
        {
            IsPrimaryKey = false;          
            IsUnique = false;
            IsNull = true;
            IsInsert = true;
            IsUpdate = true;
            Ignore = false;
        }

        public bool IsPrimaryKey { get; set; }       

        public string PropertyName { get; set; }

        public string ColumnName { get; set; }

        public object Value { get; set; }

        public string TypeName { get; set; }

        #region Filter
        public bool IsUnique { get; set; }

        public bool IsNull { get; set; }

        public bool IsInsert { get; set; }

        public bool IsUpdate { get; set; }

        public bool Ignore { get; set; }
        #endregion
    }
}
