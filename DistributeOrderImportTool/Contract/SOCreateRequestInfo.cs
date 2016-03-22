using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace DistributeOrderImportTool.Contract
{
    /// <summary>
    /// 订单创建请求信息
    /// </summary>
    public class SOCreateRequestInfo
    {
        /// <summary>
        /// 渠道编号
        /// </summary>
        public int SaleChannelSysNo { get; set; }

        /// <summary>
        /// 订单在商家销售平台上的唯一编号
        /// </summary>
        public string MerchantOrderID { get; set; }

        /// <summary>
        /// 进口模式
        /// </summary>
        public string ServerType { get; set; }

        /// <summary>
        /// 订单出库仓库在Kjt平台的编号
        /// </summary>
        public int WarehouseID { get; set; }

        /// <summary>
        /// 干线物流ID（前期由阿布才提出，这类订单，出关到国内时将第中转物流处理。
        /// </summary>
        public int ArteryLogisticID { get; set; }

        /// <summary>
        /// 订单支付信息
        /// </summary>
        public SOPayInfo PayInfo { get; set; }

        /// <summary>
        /// 订单配送信息 
        /// </summary>
        public SOShippingInfo ShippingInfo { get; set; }

        /// <summary>
        /// 购买人实名认证信息
        /// </summary>
        public SOAuthenticationInfo AuthenticationInfo { get; set; }

        /// <summary>
        /// 订单中购买商品列表
        /// </summary>
        public List<SOItemInfo> ItemList { get; set; }

        /// <summary>
        /// 新增购汇状态 默认为0，表示由KJT进行购汇；如果商户填写为1，表示商户自行购汇
        /// </summary>
        public int IsMerchantSelfFEP { get; set; }
    }

    /// <summary>
    /// 订单支付信息
    /// </summary>
    public class SOPayInfo
    {
        /// <summary>
        /// 商品总金额
        /// </summary>
        public decimal? ProductAmount { get; set; }

        /// <summary>
        /// 运费总金额
        /// </summary>
        public decimal? ShippingAmount { get; set; }

        /// <summary>
        /// 商品行邮税总金额 
        /// </summary>
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// 支付产生的手续费
        /// </summary>
        public decimal? CommissionAmount { get; set; }

        /// <summary>
        /// 支付方式编号 
        /// </summary>
        public int? PayTypeSysNo { get; set; }

        /// <summary>
        /// 支付流水号，不能重复
        /// </summary>
        public string PaySerialNumber { get; set; }

    }

    /// <summary>
    /// 订单配送信息
    /// </summary>
    public class SOShippingInfo
    {
        /// <summary>
        /// 收件人姓名 
        /// </summary>
        public string ReceiveName { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string ReceivePhone { get; set; }

        /// <summary>
        /// 收件详细地址（需要包含收件地省市区名称）
        /// </summary>
        public string ReceiveAddress { get; set; }

        /// <summary>
        /// 收件省市区名称（省市区名称之间用半角逗号,隔开，如  上海,上海市,静安区）
        /// </summary>
        public string ReceiveAreaName { get; set; }
        /// <summary>
        /// Kjt提供的收货地区编码
        /// </summary>
        public string ReceiveAreaCode { get; set; }

        /// <summary>
        /// 收件地邮政编码
        /// </summary>
        public string ReceiveZip { get; set; }

        /// <summary>
        /// 订单物流运输公司编号
        /// </summary>
        public int ShipTypeID { get; set; }
    }

    /// <summary>
    /// 订单购买人实名认证信息
    /// </summary>
    public class SOAuthenticationInfo
    {
        /// <summary>
        /// 购买人真实姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 购买人证件类型 
        /// </summary>
        public int IDCardType { get; set; }

        /// <summary>
        /// 购买人证件编号
        /// </summary>
        public string IDCardNumber { get; set; }

        /// <summary>
        /// 购买人联系电话
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 购买人电子邮件地址
        /// </summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// 订单商品信息
    /// </summary>
    public class SOItemInfo
    {
        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// KJT商品ID
        /// </summary>
        public string ProductID { get; set; }

        public decimal SalePrice { get; set; }

        public decimal TaxPrice { get; set; }
    }
}
