using System;

namespace Ro.Data.SqlClient
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    public class ColumnAttribute : Attribute
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private bool _isUnique = false;
        /// <summary>
        /// 是否是唯一的,默认为否
        /// </summary>
        public bool IsUnique
        {
            get { return _isUnique; }
            set { _isUnique = value; }
        }

        private bool _isNull = true;
        /// <summary>
        /// 此字段是否允许为空,默认允许为空
        /// </summary>
        public bool IsNull
        {
            get { return _isNull; }
            set { _isNull = value; }
        }

        private bool _isInsert = true;
        /// <summary>
        /// 在执行插入操作时此是否插入此字段值,默认为插入
        /// </summary>
        public bool IsInsert
        {
            get { return _isInsert; }
            set { _isInsert = value; }
        }

        private bool _isUpdate = true;
        /// <summary>
        /// 在执行更新操作时是否更新此字段值,默认为更新
        /// </summary>
        public bool IsUpdate
        {
            get { return _isUpdate; }
            set { _isUpdate = value; }
        }

        private bool _ignore = false;
        /// <summary>
        /// 在执行所有操作时是否忽略此字段,默认不忽略
        /// </summary>
        public bool Ignore
        {
            get { return _ignore; }
            set { _ignore = value; }
        }

        public ColumnAttribute()
        {

        }

        public ColumnAttribute(string name)
            : this()
        {
            this.Name = name;
        }

        public ColumnAttribute(string name, bool ignore)
            : this(name)
        {
            this.Ignore = ignore;
        }

        public ColumnAttribute(string name, bool isInsert, bool isUpdate)
            : this(name)
        {
            this.IsInsert = isInsert;
            this.IsUpdate = isUpdate;
        }
    }
}
