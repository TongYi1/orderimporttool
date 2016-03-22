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
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace DistributeOrderImportTool.Forms
{
    public partial class FormFEPBillPost : Form
    {

        private FEPBillPostProcessor FEPBillPostBiz = null;

        private FEPBillPostResponseResult ResultData = null;

        public FormFEPBillPost()
        {
            InitializeComponent();
            ResultData = new FEPBillPostResponseResult();
            FEPBillPostBiz = new FEPBillPostProcessor();
            FEPBillPostBiz.PostComplateEvent += FEPBillPostBiz_PostComplateEvent;
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

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSOFilePath.Text))
            {
                lblChannelInfo.Text = "请先选择结算账单模板文件！";
                return;
            }

            this.Invoke(new Action(() =>
            {

                FEPBillPostBiz.Post(tbSOFilePath.Text);
            }));
        }


        void FEPBillPostBiz_PostComplateEvent(object sender, FEPBillPostResponseResult result)
        {
            this.Invoke(new Action<FEPBillPostResponseResult>(e =>
            {
                string message = string.Format("结算账单创建失败：{0}", e.Desc);
                if (e.Code == 0)
                {
                    ResultData.Data = e.Data;
                    message = string.Format("结算账单创建成功, 结算账单号: {0}， 结算金额：{1:F2}", e.Data.FEPBillId, e.Data.PurchasingTotalAmount);
                }

                txtBoxLog.Text += message + "\r\n";
            }), result);
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "选择需要导入的Excel模板文件";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Excel文件(*.xlsx)|*.xlsx";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                tbSOFilePath.Text = openFileDialog1.FileName;
            }
        }

    }
}
