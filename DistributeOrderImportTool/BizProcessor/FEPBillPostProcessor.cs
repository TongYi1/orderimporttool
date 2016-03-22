using DistributeOrderImportTool.Contract;
using DistributeOrderImportTool.Entity;
using DistributeOrderImportTool.Utils;
using MS360.Utility;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DistributeOrderImportTool.BizProcessor
{
    public class FEPBillPostProcessor
    {
        private event EventHandler<FEPBillPostResponseResult> m_PostComplateEvent;

        public event EventHandler<FEPBillPostResponseResult> PostComplateEvent
        {
            add
            {
                m_PostComplateEvent += value;
            }
            remove
            {
                m_PostComplateEvent -= value;
            }
        }
        protected virtual void OnPostComplate(object sender, FEPBillPostResponseResult e)
        {
            var handler = m_PostComplateEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void Post(string templateFilePath)
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
                throw new BusinessException("结算账单模板文件格式错误");
            }

            List<int> docData = ReadDocumentToList(templateFilePath);
            if (docData == null || docData.Count <= 0)
            {
                return;
            }

            FEPBillPostRequestInfo contractInfo = new FEPBillPostRequestInfo()
            {
                SalesChannelCode = configData.SaleChannelSysNo,
                OrderIds = docData.ToArray()
            };

            AsyncFEPBillPost(contractInfo, configData);
        }

        private List<int> ReadDocumentToList(string templateFilePath)
        {
            List<int> docData = new List<int>();

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
                    docData.Add(int.Parse(row.GetCell(1).ToString()));
                }
            }
            return docData;
        }

        private async void AsyncFEPBillPost(FEPBillPostRequestInfo contractInfo, ConfigData configData)
        {
            FEPBillPostResponseResult result = new FEPBillPostResponseResult()
            {
                Data = new FEPBillPostResponseData() 
            };

            var parameters = new NameValueCollection();
            parameters.Add("method", "Invoice.FEPBillPost");
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
            var httpContent = new StringContent(content);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var httpClient = new HttpClient();
            try
            {
                var task = await httpClient.PostAsync(new Uri(configData.OpenApiUrl), httpContent);
                var responseData = await task.Content.ReadAsStringAsync();

                result = JsonSerializer.Deserialize<FEPBillPostResponseResult>(responseData);
                if (result.Data == null)
                {
                    result.Data = new FEPBillPostResponseData();
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

            OnPostComplate(this, result);
        }

        private void GetInnerException(Exception iEx, out Exception oEx)
        {
            oEx = iEx;
            if (iEx != null && iEx.InnerException != null)
            {
                GetInnerException(iEx.InnerException, out oEx);
            }
        }
    }
}
