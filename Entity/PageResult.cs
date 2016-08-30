using System.Collections.Generic;

namespace Ro.Data.SqlClient
{	
    public class PageResult<T>
    {       
        public int TotalCount {get; set;}
         
        public List<T> DataSource {get; set;}
    }
}
