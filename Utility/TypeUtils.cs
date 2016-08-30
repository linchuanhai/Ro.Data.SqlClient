using System;

namespace Ro.Data.SqlClient
{	
    public class TypeUtils
    {		 
        public static object ConvertForType(object value,Type type)
        {
            if (Convert.IsDBNull(value) || (value == null))
                return null;

            var typeName = type.FullName.ToString();

            if (type == typeof(System.Nullable<UInt16>))
            {
                value = Convert.ToUInt16(value);
            }
            else if (type == typeof(System.Nullable<UInt32>))
            {
                value = Convert.ToUInt32(value);
            }
            else if (type == typeof(System.Nullable<UInt64>))
            {
                value = Convert.ToUInt64(value);
            }
            else if (type == typeof(System.Nullable<Int32>))
            {
                value = Convert.ToInt32(value);
            }
            else if (type == typeof(System.Nullable<Int64>))
            {
                value = Convert.ToInt64(value);
            }
            else if (type == typeof(System.Nullable<DateTime>))
            {
                value = Convert.ToDateTime(value);
            }

            switch (typeName)
            {
                case "System.string":
                    if (!isNullOrEmpty(value))
                        value = value.ToString();
                    break;
                case "System.Boolean":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToBoolean(value);
                    break;
                case "System.Int16":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToInt16(value);
                    break;
                case "System.Int32":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToInt32(value);
                    break;
                case "System.Int64":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToInt64(value);
                    break;
                case "System.Double":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToDouble(value);
                    break;
                case "System.Float":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToDouble(value);
                    break;
                case "System.Single":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToSingle(value);
                    break;
                case "System.Decimal":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToDecimal(value);
                    break;
                case "System.DateTime":
                    if (!isNullOrEmpty(value))
                        value = Convert.ToDateTime(value);
                    break;
            }

            return value;
        }
		
        public static string GetTypeName(Type type)
        {
            if (type == null) 
                return "System.Int32";

            var typeName = type.FullName.ToString();

            if (type == typeof(System.Nullable<UInt16>))
            {
                typeName = "System.UInt16";
            }
            else if (type == typeof(System.Nullable<UInt32>))
            {
                typeName = "System.UInt32";
            }
            else if (type == typeof(System.Nullable<UInt64>))
            {
                typeName = "System.UInt64";
            }
            else if (type == typeof(System.Nullable<Int16>))
            {
                typeName = "System.Int16";
            }
            else if (type == typeof(System.Nullable<Int32>))
            {
                typeName = "System.Int32";
            }
            else if (type == typeof(System.Nullable<Int64>))
            {
                typeName = "System.Int64";
            }
            else if (type == typeof(System.Nullable<DateTime>))
            {
                typeName = "System.DateTime";
            }
            return typeName;
        }
		
        static bool isNullOrEmpty(object val)
        {
            if (val == null) 
                return true;

            if (val.ToString() == "")
                return true;

            return false;
        }
    }
}
