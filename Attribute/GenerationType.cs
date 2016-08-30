using System;

namespace Ro.Data.SqlClient
{
	/// <summary>
	/// 数据库表主键生成类型定义
	/// </summary>
    public class GenerationType 
    {
		/// <summary>
		/// 自动增长型
		/// </summary>
        public const int Indentity = 1;
		/// <summary>
		/// GUID型
		/// </summary>
        public const int Guid = 2;
		/// <summary>
		/// 提前生成并填充
		/// </summary>
        public const int Fill = 3;

        private GenerationType() { }
    }
}
