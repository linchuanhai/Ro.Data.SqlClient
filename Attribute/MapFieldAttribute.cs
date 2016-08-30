using System;

namespace Ro.Data.SqlClient
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    public class MapFieldAttribute : Attribute
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public MapFieldAttribute()
        {

        }
        public MapFieldAttribute(string name)
            :this()
        {
            this.Name = name;
        }        
    }
}
