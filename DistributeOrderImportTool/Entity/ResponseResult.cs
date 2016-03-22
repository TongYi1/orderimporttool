using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributeOrderImportTool.Entity
{
    public class ResponseResult<T>
    {
        public int Code { get; set; }
        public string Desc { get; set; }

        public T Data { get; set; }
    }
}
