using DistributeOrderImportTool.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DistributeOrderImportTool
{
    public partial class FormMain : Form
    {
        FormImportSO frmImportSO;
        FormFEPBillPost frmFEPBillPost;
        FormConfig frmConfig;
        int CHILDFORM_WIDTH = 620;
        int CHILDFORM_HEIGHT = 420;

        public FormMain()
        {
            InitializeComponent();
        }

        

        private void FormMain_Load(object sender, EventArgs e)
        {
            toolStripStatusLabeltTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabeltTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_Import_Click(object sender, EventArgs e)
        {
            if (frmImportSO == null || frmImportSO.IsDisposed)
            {
                frmImportSO = new FormImportSO ();
                frmImportSO.MdiParent = this;
                frmImportSO.Width = CHILDFORM_WIDTH;
                frmImportSO.Height = CHILDFORM_HEIGHT;
            }
           
       
            frmImportSO.WindowState = FormWindowState.Maximized;
            frmImportSO.Show();
            frmImportSO.Activate();
        }

        private void toolStripButton_Config_Click(object sender, EventArgs e)
        {
            if (frmConfig == null || frmConfig.IsDisposed)
            {
                frmConfig = new  FormConfig ();
                frmConfig.MdiParent = this;
                frmConfig.Width = CHILDFORM_WIDTH;
                frmConfig.Height = CHILDFORM_HEIGHT;
            }

          

            frmConfig.WindowState = FormWindowState.Maximized;

            frmConfig.Show();
            frmConfig.Activate();
        }

        private void toolStripButton_FEPBill_Click(object sender, EventArgs e)
        {
            if (frmFEPBillPost == null || frmFEPBillPost.IsDisposed)
            {
                frmFEPBillPost = new FormFEPBillPost();
                frmFEPBillPost.MdiParent = this;
                frmFEPBillPost.Width = CHILDFORM_WIDTH;
                frmFEPBillPost.Height = CHILDFORM_HEIGHT;
            }


            frmFEPBillPost.WindowState = FormWindowState.Maximized;
            frmFEPBillPost.Show();
            frmFEPBillPost.Activate();
        }
    }
}
