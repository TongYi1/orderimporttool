using DistributeOrderImportTool.BizProcessor;
using MS360.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DistributeOrderImportTool.Forms
{
    public partial class FormConfig : Form
    {
        
        public FormConfig()
        {
            InitializeComponent();
        }

        private void FormConfig_Load(object sender, EventArgs e)
        {
            ConfigData config = ConfigDataProcessor.GetConfigData();
            if (config != null)
            {
                tbOpenApiUrl.Text = config.OpenApiUrl;
                tbAppId.Text = config.AppId;
                tbSaleChannelSysNo.Text = config.SaleChannelSysNo.ToString();
                tbSecretKey.Text = config.SecretKey;

            }


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(tbOpenApiUrl.Text))
            {
                MessageBox.Show("OpenAPI Url不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbSaleChannelSysNo.Text))
            {
                MessageBox.Show("分销渠道编号不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbAppId.Text))
            {
                MessageBox.Show("AppID不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbSecretKey.Text))
            {
                MessageBox.Show("验签密钥不能为空！");
                return;
            }
            int channelNo=0;
            if (!int.TryParse(tbSaleChannelSysNo.Text.Trim(),out channelNo))
            {
                MessageBox.Show("分销渠道编号必须为数字！");
                return;
            }
            

            ConfigData config = new ConfigData();
            config.OpenApiUrl = tbOpenApiUrl.Text.Trim();
            config.SaleChannelSysNo = int.Parse(tbSaleChannelSysNo.Text.Trim());
            config.AppId = tbAppId.Text.Trim();
            config.SecretKey = tbSecretKey.Text.Trim();

            ConfigDataProcessor.SaveConfig(config);
            MessageBox.Show("保存配置成功！");
            

        }
    }
}
