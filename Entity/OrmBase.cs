using System;
using System.Collections.Generic;

namespace Ro.Data.SqlClient
{
    [AopAttribute]
    public class OrmBase : ContextBoundObject
    {

        private object locker = new object();
        /// <summary>
        /// 针对Nullable<T>类型强制标示为更新字段补丁方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void ForceUpdate(string name, object value)
        {
            if (ChangeFields == null)
                ChangeFields = new Dictionary<string, object>();
            if (!ChangeFields.ContainsKey(name))
                ChangeFields.Add(name, value);
        }

        public Dictionary<string, object> OriginalFields=null;

        public Dictionary<string, object> ChangeFields=null;
          
        public void SetChange(string name, object value)
        {           
            lock(locker)
            {
                try
                {
                    if (OriginalFields == null)
                        OriginalFields = new Dictionary<string, object>();

                    if (!OriginalFields.ContainsKey(name))
                    {
                        if (ChangeFields == null)
                        {
                            OriginalFields.Add(name, value);
                        }
                        else
                        {
                            if (ChangeFields == null)
                                ChangeFields = new Dictionary<string, object>();

                            if (!ChangeFields.ContainsKey(name))
                                ChangeFields.Add(name, value);
                        }
                    }
                    else
                    {
                        if (ChangeFields == null)
                            ChangeFields = new Dictionary<string, object>();

                        var originalValue = OriginalFields[name];
                        if (originalValue == null)
                        {
                            if (!ChangeFields.ContainsKey(name))
                                ChangeFields.Add(name, value);
                        }
                        else if (!ChangeFields.ContainsKey(name) && !originalValue.Equals(value))
                        {
                            ChangeFields.Add(name, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Write("OrmBase发生异常：" + ex.ToString());
                }
            }                      
        }    
    }
}
