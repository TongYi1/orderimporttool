using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributeOrderImportTool.Entity
{
    public class FEPBillPostResponseResult : ResponseResult<FEPBillPostResponseData>
    {
    }

    public class FEPBillPostResponseData
    {
        /// <summary>
        /// 待购汇账单编号
        /// </summary>
        public int FEPBillId { get; set; }
        /// <summary>
        /// 渠道结算金额
        /// </summary>
        public decimal PurchasingTotalAmount { get; set; }
    }
}
