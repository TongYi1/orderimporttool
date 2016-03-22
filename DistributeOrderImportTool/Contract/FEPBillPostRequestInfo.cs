using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributeOrderImportTool.Contract
{
    public class FEPBillPostRequestInfo
    {
        public int[] OrderIds { get; set; }

        public int SalesChannelCode { get; set; }
    }
}
