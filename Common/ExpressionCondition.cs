using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Ro.Data.SqlClient
{
    public class ExpressionCondition
    {   
        public ExpressionCondition SetWhere<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> predicate) where T : class, new()
        {
            sbSQL.Append(" WHERE  1=1 ");
            var sql = GetWhere(predicate);
            if(!string.IsNullOrEmpty(sql))           
                sbSQL.Append(" And "+ sql);
            OrderByFields = GetOrderBy(predicate);           
            return this;
        }

        public ExpressionCondition AndWhere<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> predicate) where T : class, new()
        {           
            var sql = GetWhere(predicate);
            if (!string.IsNullOrEmpty(sql))
                sbSQL.Append(" And " + sql);
            OrderByFields = GetOrderBy(predicate);
            return this;
        }

        public ExpressionCondition OrWhere<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> predicate) where T : class, new()
        {
            var sql = GetWhere(predicate);
            if (!string.IsNullOrEmpty(sql))
                sbSQL.Append(" Or " + sql);
            OrderByFields = GetOrderBy(predicate);
            return this;
        }

        public override string ToString()
        {
            return sbSQL.ToString();
        }

        public string OrderByFields;
        
        private StringBuilder sbSQL = new StringBuilder();
    
        private string GetWhere<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> predicate) where T : class, new()
        {
            var whereSql = ExpressionWriterSql.BizWhereWriteToString(predicate, ExpSqlType.Where);
           
            var regexNotExist = new Regex(@"\!(?<name>\w+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexNotExist.Replace(whereSql, "${name}=0");

            var regexExistStart = new Regex(@"\((?<name>\w+)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexExistStart.Replace(whereSql, "(${name}=1)");

            var regexExistStart2 = new Regex(@"\((?<name>\w+)\s+(?<join>(and|or))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexExistStart2.Replace(whereSql, "(${name}=1 ${join}");

            var regexExistEnd = new Regex(@"(?<name>\w+)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexExistEnd.Replace(whereSql, "${name}=1)");

            var regexFix = new Regex(@"=\s*(\w+)=(\1)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexFix.Replace(whereSql, "=$1");

            var regexFix2 = new Regex(@"(\w+)\s*=\s*(\d+)(\1)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexFix2.Replace(whereSql, "$1=$2");

            var regexFix3 = new Regex(@"=\s*(?<value>\d+)=\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            whereSql = regexFix3.Replace(whereSql, "=${value}");

            return whereSql;
        }

        private string GetOrderBy(Expression predicate)
        {
            var orderbySql = ExpressionWriterSql.BizWhereWriteToString(predicate, ExpSqlType.Order);
            if (!string.IsNullOrEmpty(orderbySql))
                orderbySql = " Order by " + orderbySql;
            return orderbySql;
        }
    }
}
