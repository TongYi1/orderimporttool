using DistributeOrderImportTool.BizProcessor;
using DistributeOrderImportTool.Entity;
using DistributeOrderImportTool.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace DistributeOrderImportTool.Forms
{
    public partial class FormImportSO : Form
    {

        private ImportOrderProcessor OrderImportBiz = null;

        private List<OrderCreateResponseData> ResultData = null;

        public FormImportSO()
        {
            InitializeComponent();
            ResultData = new List<OrderCreateResponseData>();
            OrderImportBiz = new ImportOrderProcessor();
            OrderImportBiz.OrderImportComplateEvent += processor_OrderImportComplateEvent;
            OrderImportBiz.ImportFileReadComplateEvent += OrderImportBiz_ImportFileReadComplateEvent;
        }

        private void FormImportSO_Load(object sender, EventArgs e)
        {
            ConfigData config = ConfigDataProcessor.GetConfigData();
            if (config != null)
            {
                lblChannelInfo.Text = string.Format("当前导入分销渠道编号为{0},商户AppId为{1};导入时将先对数据进行检查!", config.SaleChannelSysNo, config.AppId);
            }
            else
            {
                lblChannelInfo.Text = "请先进入配置管理中，配置正确您的分销渠道相关信息！";
            }
        }

        void OrderImportBiz_ImportFileReadComplateEvent(object sender, int e)
        {
            progressBar1.Maximum = e;
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSOFilePath.Text))
            {
                lblChannelInfo.Text = "请先选择订单导入模板文件！";
                return;
            }

            btnImportFile.Enabled = false;
            progressBar1.Maximum = 0;
            progressBar1.Value = 0;

            string templateFilePath = tbSOFilePath.Text;
            Thread thread = new Thread(new ThreadStart(() => OrderImportBiz.Import(templateFilePath)));
            thread.Start();
        }


        void processor_OrderImportComplateEvent(object sender, OrderCreateResponseResult result)
        {
            this.Invoke(new Action<OrderCreateResponseResult>(e =>
            {

                progressBar1.Value += 1;
                string message = string.Format("订单{0}创建失败：{1}", e.Data.MerchantOrderID, e.Desc);
                if (e.Code == 0)
                {
                    ResultData.Add(result.Data);
                    message = string.Format("订单{0}创建成功：{1}", e.Data.MerchantOrderID, JsonSerializer.Serialize(e.Data));
                }

                listBoxLog.Items.Add(new ListItem(message));

                if (progressBar1.Value == progressBar1.Maximum)
                {
                    btnImportFile.Enabled = true;
                    this.btnExportFile_Click(this, null);
                }
            }), result);
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "选择需要导入的Excel模板文件";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "所有文件(*.*)|*.*|Excel文件(*.xls)|*.xls|Excel文件(*.xlsx)|*.xlsx";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                tbSOFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void btnExportFile_Click(object sender, EventArgs e)
        {
            var data = OrderImportBiz.ExportToFile(ResultData);

            saveFileDialog1.Filter = "excel files(*.xlsx)|*.xlsx";
            saveFileDialog1.RestoreDirectory = true;
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                using (Stream fs = saveFileDialog1.OpenFile())
                {
                    fs.Write(data, 0, data.Length);
                }
            }
        }

    }
}
