using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ro.Data.SqlClient
{
    public static class ReflectionUtils
    {
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        #region  Fast Method Invoke
        public static object FastMethodInvoke(object obj, MethodInfo method, params object[] parameters)
        {
            return DynamicCalls.GetMethodInvoker(method)(obj, parameters);
        }

        public static T Create<T>()
        {
            return (T)Create(typeof(T))();
        }

        public static FastCreateInstanceHandler Create(Type type)
        {
            return DynamicCalls.GetInstanceCreator(type);
        }
        #endregion

        #region Set Property Value
        public static void SetPropertyValue(object obj, PropertyInfo property, object value)
        {
            if (property.CanWrite)
            {
                var propertySetter = DynamicCalls.GetPropertySetter(property);
                value = TypeUtils.ConvertForType(value, property.PropertyType);

                if (value != null)
                {
                    propertySetter(obj, value);
                }                                 
            }
        }

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            SetPropertyValue(obj.GetType(), obj, propertyName, value);
        }

        public static void SetPropertyValue(Type type, object obj, string propertyName, object value)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            if (property != null)
                SetPropertyValue(obj, property, value);
        }

        public static object GetPropertyValue<T>(T entity, string propertyName)
        {
            PropertyInfo property = entity.GetType().GetProperty(propertyName);
            if (property != null && property.CanRead)
                return property.GetValue(entity, null);
            return null;
        }

        public static object[] GetPropertyValues<T>(T entity,List<PrimaryKeyInfo> primaryKeys)
        {
            object[] values = null;
            if (primaryKeys != null)
            {
                values = new object[primaryKeys.Count];
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var property = entity.GetType().GetProperty(primaryKeys[i].ColumnName);
                    if (property != null && property.CanRead)
                    {
                        values[i] = property.GetValue(entity, null);
                    }
                    else
                    {
                        throw new Exception("实体主键无法建立映射关系！");
                    }                       
                }              
            }
            return values;           
        }

        public static object GetPropertyValue<T>(T entity, PropertyInfo property)
        {
            if (property != null && property.CanRead)
                return property.GetValue(entity, null);
            return null;
        }

        public static Type GetPropertyType(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            if (property != null && property.CanRead)
                return property.PropertyType;
            return null;
        }
        #endregion
    }
}
