using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;


namespace Ro.Data.SqlClient
{
    public static class EntityUtils
    {
      
        public static TableInfo GetTableModel(object entity)
        {          
            TableInfo tableInfo = null;
            List<PrimaryKeyInfo> primaryKeys = null;
            var type = entity.GetType();  
           
            var properties = ReflectionUtils.GetProperties(type);
            if (properties != null && properties.Length > 0)
            {
                tableInfo = GetTableInfo(type);

                var columns = new List<ColumnInfo>();
                foreach (PropertyInfo property in properties)
                {
                    object[] propertyAttrs = property.GetCustomAttributes(false);
                    if (propertyAttrs != null && propertyAttrs.Length > 0)
                    {
                        for (int i = 0; i < propertyAttrs.Length; i++)
                        {
                            object propertyAttr = propertyAttrs[i];
                            if (IsColumn(propertyAttr))
                            {
                                var column = new ColumnInfo();
                                column.PropertyName = property.Name;
                                column.ColumnName = GetColumnName(propertyAttr);
                                column.TypeName = TypeUtils.GetTypeName(property.PropertyType);

                                if (string.IsNullOrEmpty(column.ColumnName))
                                    column.ColumnName = column.PropertyName;

                                if (propertyAttr is IdAttribute)
                                {
                                    if (primaryKeys == null)
                                        primaryKeys = new List<PrimaryKeyInfo>();

                                    var idAttribute = propertyAttr as IdAttribute;
                                    column.IsPrimaryKey = true;

                                    var primaryKeyInfo = new PrimaryKeyInfo()
                                    {
                                        ColumnName = column.ColumnName,
                                        PropertyName = property.Name,
                                        TypeName = column.TypeName
                                    };
                                    primaryKeys.Add(primaryKeyInfo);
                                }
                                if (propertyAttr is ColumnAttribute)
                                {
                                    var columnAttribute = propertyAttr as ColumnAttribute;
                                    column.IsUnique = columnAttribute.IsUnique;
                                    column.IsNull = columnAttribute.IsNull;
                                    column.IsInsert = columnAttribute.IsInsert;
                                    column.IsUpdate = columnAttribute.IsUpdate;
                                    column.Ignore = columnAttribute.Ignore;
                                }
                                columns.Add(column);
                                break;
                            }
                        }
                    } 
                } 
                tableInfo.PrimaryKeys = primaryKeys;
                tableInfo.Columns = columns;                 
            } 
            return (TableInfo)tableInfo.Clone();
        }

        public static Dictionary<string, object> GetChangeColumns<T>(T entity, TableInfo tableInfo)
        {
            var realChangeFields = new Dictionary<string, object>();           
            var type = entity.GetType();
            var properties = ReflectionUtils.GetProperties(type);
            if (properties != null && properties.Length > 0)
            {
                var field = type.GetField("ChangeFields");
                if(field!=null)
                {
                    var changeFields = new Dictionary<string, object>();
                    changeFields = (Dictionary<string, object>)field.GetValue(entity); 
                   
                    if (changeFields!=null&& changeFields.Count>0)
                    {                        
                        foreach (var pair in changeFields)
                        {
                            var name = pair.Key;
                            if (tableInfo.Columns.Any(c => c.PropertyName == name))
                            {
                                name = tableInfo.Columns.First(c => c.PropertyName == name).ColumnName;
                                realChangeFields.Add(name, pair.Value);
                            }
                        }
                    }                    
                }                    
            }
            return realChangeFields;
        }

        private static TableInfo GetTableInfo(Type entity)
        {
            var tableInfo = new TableInfo();

            var tableName = entity.Name;
            object[] objs = entity.GetCustomAttributes(false);
            if (objs.Length == 0)
            {
                tableInfo.TableName = tableName;
                tableInfo.Strategy = GenerationType.Indentity;
            }
            else
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] is TableAttribute)
                    {
                        var attribute = objs[i] as TableAttribute;
                        tableInfo.TableName = attribute.Name;
                        tableInfo.Strategy = attribute.Strategy;
                        break;
                    }
                }
            }  
            return tableInfo;
        }

        private static string GetColumnName(object attribute)
        {
            var columnName = string.Empty;
            if (attribute is ColumnAttribute)
            {
                var columnAttr = attribute as ColumnAttribute;
                columnName = columnAttr.Name;
            }
            if (attribute is IdAttribute)
            {
                var idAttr = attribute as IdAttribute;
                columnName = idAttr.Name;
            }
            return columnName;
        }       

        private static bool IsColumn(object attribute)
        {
            return (attribute is ColumnAttribute) || (attribute is IdAttribute);
        } 

        public static List<T> ToList<T>(IDataReader sdr, TableInfo tableInfo) where T : new()
        {          
            List<T> list = new List<T>();
            var properties = ReflectionUtils.GetProperties(new T().GetType());
            var columns = tableInfo.Columns;
            while (sdr.Read())
            {
                T entity = new T();
                foreach (PropertyInfo property in properties)
                {                   
                    if (columns.Any(c => c.PropertyName == property.Name))
                    {                    

                        var columnName = columns.FirstOrDefault(c => c.PropertyName == property.Name)
                                                .ColumnName;                   
                        ReflectionUtils.SetPropertyValue(entity, property, sdr[columnName]);
                    }                    
                }
                list.Add(entity);
            }
            return list;
        }

        public static List<T> ToList<T>(DataSet ds) where T : new()
        {
            if (ds == null || ds.Tables.Count == 0)
                return null;

            var dt = ds.Tables[0];          
            List<T> modelList = new List<T>();
            var columnName = string.Empty;
            try
            {
                T entity=new T();
                var tableName = entity.GetType().Name;
                var tablePropertyInfoes = GetMapFields<T>();

                if (tablePropertyInfoes != null && tablePropertyInfoes.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        T model = new T();
                        for (int i = 0; i < dr.Table.Columns.Count; i++)
                        {
                            columnName = dr.Table.Columns[i].ColumnName.ToLower();
                            if (dr[i] != DBNull.Value)
                            {
                                if (tablePropertyInfoes.ContainsKey(columnName) && tablePropertyInfoes[columnName].CanWrite)
                                    tablePropertyInfoes[columnName].SetValue(model, dr[i], null);
                            }
                        }
                        modelList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(string.Format("当前针对属性赋值发生异常，更多详细原因：{0}", ex.ToString()));
            }
            return modelList;
        }     
         
        private static Dictionary<string, PropertyInfo> GetMapFields<T>() where T : new()
        {
            var mapFields = new Dictionary<string, PropertyInfo>();
            T model = new T();
            var propertyInfoes = model.GetType().GetProperties();
            if (propertyInfoes != null && propertyInfoes.Length > 0)
            {
                foreach (var property in propertyInfoes)
                {
                    object[] objs = property.GetCustomAttributes(typeof(MapFieldAttribute), false);
                    if (objs != null && objs.Length > 0)
                    {
                        var attribute = (MapFieldAttribute)objs[0];
                        if (!mapFields.ContainsKey(attribute.Name.ToLower()))
                            mapFields.Add(attribute.Name.ToLower(), property);
                    }
                    else
                    {
                        if (!mapFields.ContainsKey(property.Name.ToLower()))
                            mapFields.Add(property.Name.ToLower(), property);
                    }
                }
            }
            return mapFields;
        }
    }
}
