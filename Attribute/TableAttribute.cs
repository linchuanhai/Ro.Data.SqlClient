using System;

namespace Ro.Data.SqlClient
{	 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {		 
        private string _name = string.Empty;

        public TableAttribute()
        {  
                      
        }

        public TableAttribute(string name)          
        {
            this.Name = name;
            this.Strategy = GenerationType.Indentity;
        }

        public TableAttribute(string name,int strategy)           
        {
            this.Name = name;
            this.Strategy = strategy;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 主键字段生成方式,参考GenerationType定义
        /// </summary>
        public int Strategy { get; set; }
    }
}
