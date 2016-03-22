using DistributeOrderImportTool.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Entity
{
    public class OrderImportRowData
    {
        /// <summary>
        /// 商户订单编号
        /// </summary>
        [RequireField]
        [LimitLength(50)]
        public OrderImportCellData MerchantOrderID { get; set; }

        /// <summary>
        /// 进口模式
        /// </summary>
        [RequireField]
        public OrderImportCellData ServerType { get; set; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        [RequireField]
        [IntField]
        public OrderImportCellData WarehouseID { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        [RequireField]
        public OrderImportCellData PayTypeID { get; set; }

        /// <summary>
        /// 支付流水号
        /// </summary>
        [RequireField]
        [LimitLength(28)]
        public OrderImportCellData PaySerialNumber { get; set; }


        /// <summary>
        /// 商品总金额
        /// </summary>
        [RequireField]
        [DecimalField]
        [MinValueField(0)]
        public OrderImportCellData ProductAmount { get; set; }

        /// <summary>
        /// 运费总金额
        /// </summary>
        [RequireField]
        [DecimalField]
        [MinValueField(0)]
        public OrderImportCellData ShippingAmount { get; set; }

        /// <summary>
        /// 配送方式
        /// </summary>
        [RequireField]
        [IntField]
        public OrderImportCellData ShipTypeID { get; set; }

        /// <summary>
        /// 收件人姓名 
        /// </summary>
        [RequireField]
        [LimitLength(20)]
        public OrderImportCellData ReceiveName { get; set; }

        /// <summary>
        /// 收件人手机电话
        /// </summary>
        [RequireField]
        [RegexField("^1(\\d{10})$")]
        public OrderImportCellData ReceiveCellPhone { get; set; }

        /// <summary>
        /// 收件人省市区
        /// </summary>
        [RequireField]
        public OrderImportCellData ReceiveAreaName { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        [RequireField]
        [LimitLength(100)]
        public OrderImportCellData ReceiveAddress { get; set; }

        /// <summary>
        /// 收货邮编
        /// </summary>
        [RequireField]
        [RegexField("^(\\d{6})$")]
        public OrderImportCellData ReceiveZip { get; set; }


        /// <summary>
        /// 实名认证姓名
        /// </summary>
        [RequireField]
        [LimitLength(20)]
        public OrderImportCellData AuthName { get; set; }



        /// <summary>
        /// 身份证号码
        /// </summary>
        [RequireField]
        [RegexField("^(\\d{17})([\\dxX]{1}$)")]
        public OrderImportCellData IDCardNumber { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [RequireField]
        [RegexField("^1(\\d{10})$")]
        public OrderImportCellData AuthCellPhone { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [RequireField]
        [RegexField("^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$")]
        public OrderImportCellData AuthEmail { get; set; }

        /// <summary>
        /// 商品ID(多个用逗号分隔)
        /// </summary>
        [RequireField]
        public OrderImportCellData ProductIDs { get; set; }

        /// <summary>
        /// 购买数量(多个用逗号分隔)
        /// </summary>
        [RequireField]
        public OrderImportCellData ProductQtys { get; set; }

        /// <summary>
        /// 商品价格(多个用逗号分隔)
        /// </summary>
        [RequireField]
        public OrderImportCellData ProductPrices { get; set; }

        /// <summary>
        /// 快递干线ID，阿布才的干线订单请传1；如果不需要干线物流，请传0
        /// </summary>
        [RequireField]
        [IntField]
        public OrderImportCellData ArteryLogisticID { get; set; }

        /// <summary>
        /// 新增购汇状态 默认为0，表示由KJT进行购汇；如果商户填写为1，表示商户自行购汇
        /// </summary>
        [IntField]
        public OrderImportCellData IsMerchantSelfFEP { get; set; }
    }
}
