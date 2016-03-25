using DistributeOrderImportTool.Contract;
using DistributeOrderImportTool.Entity;
using DistributeOrderImportTool.Utils;
using DistributeOrderImportTool.Validator;
using MS360.Utility;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DistributeOrderImportTool.BizProcessor
{
    public class ImportOrderProcessor
    {
        private event EventHandler<OrderCreateResponseResult> m_OrderImportComplateEvent;

        public event EventHandler<OrderCreateResponseResult> OrderImportComplateEvent
        {
            add
            {
                m_OrderImportComplateEvent += value;
            }
            remove
            {
                m_OrderImportComplateEvent -= value;
            }
        }

        private event EventHandler<int> m_ImportFileReadComplateEvent;

        public event EventHandler<int> ImportFileReadComplateEvent
        {
            add
            {
                m_ImportFileReadComplateEvent += value;
            }
            remove
            {
                m_ImportFileReadComplateEvent -= value;
            }
        }

        protected virtual void OnOrderImportComplate(object sender, OrderCreateResponseResult e)
        {
            var handler = m_OrderImportComplateEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected virtual void OnOrderImportFileReadComplate(object sender, int e)
        {
            var handler = m_ImportFileReadComplateEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void Import(string templateFilePath)
        {
            ConfigData configData = ConfigDataProcessor.GetConfigData();
            if (configData == null)
            {
                throw new BusinessException("配置不存在，请先保存基础配置");
            }

            if (!File.Exists(templateFilePath))
            {
                throw new BusinessException("文件\"{0}\"不存在", templateFilePath);
            }
            string ext = Path.GetExtension(templateFilePath);
            if (string.Compare(ext, ".xlsx", true) != 0)
            {
                throw new BusinessException("订单模板文件格式错误");
            }

            List<OrderImportRowData> docData = ReadDocumentToList(templateFilePath);
            OnOrderImportFileReadComplate(this, docData == null ? 0 : docData.Count);

            if (docData == null || docData.Count <= 0)
            {
                return;
            }


            string frequencyCfg = ConfigurationManager.AppSettings["order.socreate.frequency"];
            double frequencyValue;
            TimeSpan frequency = TimeSpan.FromSeconds(0);
            if (!string.IsNullOrWhiteSpace(frequencyCfg) && double.TryParse(frequencyCfg, out frequencyValue))
            {
                frequency = TimeSpan.FromSeconds(frequencyValue);
            }

            int millisecondsDelay = (int)frequency.TotalMilliseconds;
            string errorMsg = null;
            foreach (var item in docData)
            {
                if (!ValidateData(item, out errorMsg))
                {
                    OnOrderImportComplate(this, new OrderCreateResponseResult()
                    {
                        Code = -1,
                        Data = new OrderCreateResponseData() { MerchantOrderID = item.MerchantOrderID.Value },
                        Desc = errorMsg
                    });
                }
                else
                {
                    SOCreateRequestInfo contractInfo = Convert2ContractInfo(item);
                    contractInfo.SaleChannelSysNo = configData.SaleChannelSysNo;
                    ImportOrder(contractInfo, configData);
                }
                Thread.Sleep(millisecondsDelay);
            }
        }

        private  void ImportOrder(SOCreateRequestInfo contractInfo, ConfigData configData)
        {
            OrderCreateResponseResult result = new OrderCreateResponseResult()
            {
                Data = new OrderCreateResponseData() { MerchantOrderID = contractInfo.MerchantOrderID }
            };

            var parameters = new NameValueCollection();
            parameters.Add("method", "Order.SOCreate");
            parameters.Add("data", JsonSerializer.Serialize(contractInfo));
            parameters.Add("format", "json");
            parameters.Add("version", "1.0");
            parameters.Add("nonce", new Random().NextDouble().ToString());
            parameters.Add("appid", configData.AppId);
            parameters.Add("timestamp", DateTime.Now.ToString("yyyyMMddhhmmss"));

            string sign = SignatureUtil.Build(parameters, configData.SecretKey);

            StringBuilder httpContentStringBuilder = new StringBuilder();
            foreach (string key in parameters.AllKeys)
            {
                httpContentStringBuilder.AppendFormat("&{0}={1}", key, SignatureUtil.UrlEncode(parameters[key]));
            }
            httpContentStringBuilder.AppendFormat("&{0}={1}", "sign", sign);
            var content = httpContentStringBuilder.ToString().TrimStart('&');

            WebRequest request = WebRequest.Create(configData.OpenApiUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
           using( Stream requestStream = request.GetRequestStream())
           {
               byte[] postData = Encoding.UTF8.GetBytes(content);
               requestStream.Write(postData, 0, postData.Length);
           }
            try
            {
                string responseData = "";
                WebResponse response=  request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    using(StreamReader reader =  new StreamReader(responseStream))
                    {
                        responseData = reader.ReadToEnd();
                    }
                }
                result = JsonSerializer.Deserialize<OrderCreateResponseResult>(responseData);
                if (result.Data == null)
                {
                    result.Data = new OrderCreateResponseData() { MerchantOrderID = contractInfo.MerchantOrderID };
                }
            }
            catch (AggregateException aex)
            {
                result.Code = -10;
                var egex = (AggregateException)aex;
                Exception innerExp = null;
                if (egex.InnerExceptions != null && egex.InnerExceptions.Count > 0)
                {
                    innerExp = egex.InnerExceptions[0];
                }
                if (innerExp == null)
                {
                    result.Desc = egex.Message;
                }
                else
                {
                    Exception tempExp;
                    GetInnerException(innerExp, out tempExp);
                    if (tempExp != null)
                    {
                        result.Desc = tempExp.Message;
                    }
                    else
                    {
                        result.Desc = innerExp.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -10;
                result.Desc = ex.Message;
            }

            OnOrderImportComplate(this, result);
        }

        private void GetInnerException(Exception iEx, out Exception oEx)
        {
            oEx = iEx;
            if (iEx != null && iEx.InnerException != null)
            {
                GetInnerException(iEx.InnerException, out oEx);
            }
        }
       
        private SOCreateRequestInfo Convert2ContractInfo(OrderImportRowData item)
        {
            SOCreateRequestInfo contractInfo = new SOCreateRequestInfo();
            contractInfo.MerchantOrderID = item.MerchantOrderID.Value;
            contractInfo.ServerType = item.ServerType.ExtractValue;
            contractInfo.WarehouseID = int.Parse(item.WarehouseID.Value);
            contractInfo.ArteryLogisticID = int.Parse(item.ArteryLogisticID.Value);
            contractInfo.IsMerchantSelfFEP = int.Parse(item.IsMerchantSelfFEP.Value);

            contractInfo.PayInfo = new SOPayInfo();
            contractInfo.PayInfo.ProductAmount = decimal.Parse(item.ProductAmount.Value);
            contractInfo.PayInfo.ShippingAmount = decimal.Parse(item.ShippingAmount.Value);
            contractInfo.PayInfo.TaxAmount = 0;
            contractInfo.PayInfo.CommissionAmount = 0;
            contractInfo.PayInfo.PayTypeSysNo = int.Parse(item.PayTypeID.ExtractValue);
            contractInfo.PayInfo.PaySerialNumber = item.PaySerialNumber.Value;

            contractInfo.ShippingInfo = new SOShippingInfo();
            contractInfo.ShippingInfo.ReceiveName = item.ReceiveName.Value;
            contractInfo.ShippingInfo.ReceivePhone = item.ReceiveCellPhone.Value;
            contractInfo.ShippingInfo.ReceiveAddress = item.ReceiveAddress.Value;
            contractInfo.ShippingInfo.ReceiveAreaName = item.ReceiveAreaName.Value.Substring(0, item.ReceiveAreaName.Value.LastIndexOf('['));
            contractInfo.ShippingInfo.ReceiveAreaCode = item.ReceiveAreaName.ExtractValue.Substring(item.ReceiveAreaName.ExtractValue.LastIndexOf(',') + 1);
            contractInfo.ShippingInfo.ReceiveZip = item.ReceiveZip.Value;
            contractInfo.ShippingInfo.ShipTypeID = int.Parse(item.ShipTypeID.Value);

            contractInfo.AuthenticationInfo = new SOAuthenticationInfo();
            contractInfo.AuthenticationInfo.Name = item.AuthName.Value;
            contractInfo.AuthenticationInfo.IDCardType = 0;
            contractInfo.AuthenticationInfo.IDCardNumber = item.IDCardNumber.Value;
            contractInfo.AuthenticationInfo.PhoneNumber = item.AuthCellPhone.Value;
            contractInfo.AuthenticationInfo.Email = item.AuthEmail.Value;

            contractInfo.ItemList = new List<SOItemInfo>();
            string[] productIDs = item.ProductIDs.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] productQtys = item.ProductQtys.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] productPrices = item.ProductPrices.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < productIDs.Length; i++)
            {
                contractInfo.ItemList.Add(new SOItemInfo()
                {
                    ProductID = productIDs[i],
                    Quantity = int.Parse(productQtys[i]),
                    SalePrice = decimal.Parse(productPrices[i]),
                    TaxPrice = 0
                });
            }

            return contractInfo;
        }

        private bool ValidateData(OrderImportRowData item, out string errorMsg)
        {
            try
            {
                errorMsg = string.Empty;
                if (!ValidationManager.Validate(item, out errorMsg))
                {
                    throw new BusinessException(errorMsg);
                }

                if (item.IDCardNumber.Value.Length != 18 || !CheckIDCard18(item.IDCardNumber.Value))
                {
                    throw new BusinessException("身份证格式不正确");
                }
                string[] productIDs = item.ProductIDs.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                string[] productQtys = item.ProductQtys.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                string[] productPrices = item.ProductPrices.Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                if (!(productIDs.Length == productQtys.Length && productQtys.Length == productPrices.Length))
                {
                    throw new BusinessException("商品ID、购买数量、商品价格 不对应");
                }

                int qtyTmp;
                for (int i = 0; i < productQtys.Length; i++)
                {
                    if (!int.TryParse(productQtys[i], out qtyTmp) || qtyTmp <= 0)
                    {
                        throw new BusinessException("商品购买数量 {0} 不正确", productQtys[i]);
                    }
                }

                decimal priceTmp;
                for (int i = 0; i < productPrices.Length; i++)
                {
                    if (!decimal.TryParse(productPrices[i], out priceTmp) || priceTmp < 0)
                    {
                        throw new BusinessException("商品价格 {0} 不正确", productPrices[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
            errorMsg = null;
            return true;

        }

        private bool CheckIDCard18(string cardNumber)
        {
            long n = 0;
            if (long.TryParse(cardNumber.Remove(17), out n) == false || n < Math.Pow(10, 16)
                || long.TryParse(cardNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(cardNumber.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = cardNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = cardNumber.Remove(17).ToCharArray();

            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != cardNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        private List<OrderImportRowData> ReadDocumentToList(string templateFilePath)
        {
            List<OrderImportRowData> docData = new List<OrderImportRowData>();

            using (FileStream fs = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workBook = new XSSFWorkbook(fs);
                ISheet sheet = workBook.GetSheetAt(0);
                int skipRowCount = 1;
                int rowIndex = skipRowCount;
                while (true)
                {
                    IRow row = sheet.GetRow(rowIndex++);
                    if (row == null || row.GetCell(0) == null || string.IsNullOrWhiteSpace(row.GetCell(0).ToString()))
                    {
                        break;
                    }
                    OrderImportRowData docRowData = new OrderImportRowData();
                    docRowData.MerchantOrderID = new OrderImportCellData("商户订单编号", row.GetCell(0).ToString());
                    docRowData.ServerType = new OrderImportCellData("进口模式", row.GetCell(1).ToString());
                    docRowData.WarehouseID = new OrderImportCellData("仓库编号", row.GetCell(2).ToString());
                    docRowData.PayTypeID = new OrderImportCellData("支付方式", row.GetCell(3).ToString());
                    docRowData.PaySerialNumber = new OrderImportCellData("支付流水号", row.GetCell(4).ToString());
                    docRowData.ProductAmount = new OrderImportCellData("商品总金额", row.GetCell(5).ToString());
                    docRowData.ShippingAmount = new OrderImportCellData("运费总金额", row.GetCell(6).ToString());
                    docRowData.ShipTypeID = new OrderImportCellData("配送方式", row.GetCell(7).ToString());
                    docRowData.ReceiveName = new OrderImportCellData("收件人姓名", row.GetCell(8).ToString());
                    docRowData.ReceiveCellPhone = new OrderImportCellData("收件人手机电话", row.GetCell(9).ToString());
                    docRowData.ReceiveAreaName = new OrderImportCellData("收件人省市区", row.GetCell(10).ToString());
                    docRowData.ReceiveAddress = new OrderImportCellData("收货地址", row.GetCell(11).ToString());
                    docRowData.ReceiveZip = new OrderImportCellData("收货邮编", row.GetCell(12)==null?"000000": row.GetCell(12).ToString());
                    docRowData.AuthName = new OrderImportCellData("实名认证姓名", row.GetCell(13).ToString());
                    docRowData.IDCardNumber = new OrderImportCellData("身份证号码", row.GetCell(14).ToString());
                    docRowData.AuthCellPhone = new OrderImportCellData("手机号码", row.GetCell(15).ToString());
                    docRowData.AuthEmail = new OrderImportCellData("电子邮箱", row.GetCell(16).ToString());
                    docRowData.ProductIDs = new OrderImportCellData("商品ID(多个用逗号分隔)", row.GetCell(17).ToString());
                    docRowData.ProductQtys = new OrderImportCellData("购买数量(多个用逗号分隔)", row.GetCell(18).ToString());
                    docRowData.ProductPrices = new OrderImportCellData("商品价格(多个用逗号分隔)", row.GetCell(19).ToString());
                    docRowData.ArteryLogisticID = new OrderImportCellData("干线ID",  row.GetCell(20)==null? "0": row.GetCell(20).ToString());
                    docRowData.IsMerchantSelfFEP = new OrderImportCellData("自行购汇", row.GetCell(21) == null ? "0" : row.GetCell(21).ToString());
                    docData.Add(docRowData);
                }
            }
            return docData;
        }

        public byte[] ExportToFile(List<OrderCreateResponseData> resultData)
        {
            if (resultData == null) resultData = new List<OrderCreateResponseData>();

            IWorkbook workBook = new XSSFWorkbook();
            ISheet sheet = workBook.CreateSheet("订单导入结果");
            sheet.SetColumnWidth(0, 15 * 256);
            sheet.SetColumnWidth(1, 15 * 256);
            sheet.SetColumnWidth(2, 15 * 256);
            sheet.SetColumnWidth(3, 15 * 256);
            sheet.SetColumnWidth(4, 20 * 256);


            ICellStyle headerStyle = workBook.CreateCellStyle();
            headerStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            IRow header = sheet.CreateRow(0);
            ICell cell = header.CreateCell(0);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("商家订单号");

            cell = header.CreateCell(1);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("Kjt系统订单号");

            cell = header.CreateCell(2);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("商品总金额");

            cell = header.CreateCell(3);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("运费总金额");

            cell = header.CreateCell(4);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("商品行邮税总金额");


            ICellStyle contentStyle = workBook.CreateCellStyle();
            contentStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            contentStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            contentStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            contentStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;

            int headerRowIndex = 1;
            foreach (var item in resultData)
            {
                IRow row = sheet.CreateRow(headerRowIndex);
                ICell dataCell = row.CreateCell(0, CellType.String);
                dataCell.CellStyle = contentStyle;
                dataCell.SetCellValue(item.MerchantOrderID);

                dataCell = row.CreateCell(1, CellType.String);
                dataCell.CellStyle = contentStyle;
                dataCell.SetCellValue(item.SOSysNo);

                dataCell = row.CreateCell(2, CellType.Numeric);
                dataCell.CellStyle = contentStyle;
                dataCell.SetCellValue(item.ProductAmount.ToString("f2"));


                dataCell = row.CreateCell(3, CellType.Numeric);
                dataCell.CellStyle = contentStyle;
                dataCell.SetCellValue(item.ShippingAmount.ToString("f2"));

                dataCell = row.CreateCell(4, CellType.Numeric);
                dataCell.CellStyle = contentStyle;
                dataCell.SetCellValue(item.TaxAmount.ToString("f2"));

                headerRowIndex++;
            }
            using (MemoryStream fs = new MemoryStream())
            {
                workBook.Write(fs);
                fs.Flush();
                return fs.ToArray();
            }

        }
    }
}