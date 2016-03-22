using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Entity
{
    public class OrderCreateResponseResult : ResponseResult<OrderCreateResponseData>
    {

    }

    public class OrderCreateResponseData
    {
        /// <summary>
        /// Kjt系统订单号
        /// </summary>
        public int SOSysNo { get; set; }

        /// <summary>
        /// 商家订单号
        /// </summary>
        public string MerchantOrderID { get; set; }

        /// <summary>
        /// 商品总金额
        /// </summary>
        public decimal ProductAmount { get; set; }

        /// <summary>
        /// 运费总金额
        /// </summary>
        public decimal ShippingAmount { get; set; }

        /// <summary>
        /// 商品行邮税总金额 
        /// </summary>
        public decimal TaxAmount { get; set; }
    }
}
