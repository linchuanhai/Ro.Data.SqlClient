using System;

namespace Ro.Data.SqlClient
{	 
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, 
        AllowMultiple = false, Inherited = false)]
    public class IdAttribute : Attribute
    {		
        private string _name = string.Empty;		
		public string Name
		{
			get { return this._name; }
			set { this._name = value; }
		}         
		 
		public IdAttribute()
		{
			
		}

		/// <summary>
		/// 创建一个制定字段名的主键特性,字段生成方式为自动增长型
		/// </summary>
		/// <param name="name">主键字段名</param>
		public IdAttribute(string name)
			: this()
		{
			this.Name = name;
		}
    }
}
