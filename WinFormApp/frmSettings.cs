using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OM;
using System.IO;

namespace WinFormApp
{
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();

            txtFilename.Text = Settings.FileName;

            LoadFile(txtFilename.Text);

            dgSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select Settings XML File";
            fileDialog.Filter = "XML Files|*.xml";
            fileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            DialogResult result = fileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                txtFilename.Text = fileDialog.FileName;
                LoadFile(txtFilename.Text);
            }
        }
        private void LoadFile(String FileName)
        {
            OM.Controller.LoadSettings(FileName);

            List<EtwEventInfo> events = OM.Controller.GetSettings();

            dgSettings.DataSource = events;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<EtwEventInfo> events = (List<EtwEventInfo>)dgSettings.DataSource ;

            OM.Controller.SaveSettings(txtFilename.Text, events);

            this.Close();
        }
    }
}
