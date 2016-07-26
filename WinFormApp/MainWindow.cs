using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using OM;

namespace WinFormApp
{
    public partial class MainWindow : Form
    {
        private string statusTotalEtwCount;
        private string statusFilterEtwCount;
        private string statusEtwDelay;
        private DateTime? firstEvent = null;
        public MainWindow()
        {
            InitializeComponent();

            cmbProcesses.DisplayMember = "Value";
            cmbProcesses.ValueMember = "Key";
            List<KeyValuePair<int, String>> processes = OM.Controller.GetProcesses();
            processes.Sort((firstPair, nextPair) =>
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
            );
            KeyValuePair<int, String> defaultProcess = new KeyValuePair<int,string>(0, "All");
            cmbProcesses.Items.Add(defaultProcess);
            cmbProcesses.SelectedIndex = 0;
            foreach (KeyValuePair<int, String> process in processes)
            {
                cmbProcesses.Items.Add(process);
            }
            
            this.MaximumSize = new Size(this.Width, this.Height);
            this.MinimumSize = new Size(this.Width, this.Height);

            lblStatusTotalEtwCount.Text = String.Empty;
            lblStatusFilterEtwCount.Text = String.Empty;
            lblStatusEtwDelay.Text = String.Empty;

            OM.Controller.LoadSettings("events.xml");
            OM.Controller.OnEtwEvent += new EventHandler<OM.EtwEventArgs>(Controller_OnEtwEvent);
            OM.Controller.OnEtwEventCounter += new EventHandler<OM.EtwEventCounterArgs>(Controller_OnEtwEventCounter);
        }

        ~MainWindow()
        {
            if (running)
            {
                OM.Controller.Stop();
            }
        }

        private TimeSpan uiDelay = TimeSpan.FromMilliseconds(100);
        private DateTime uiLastPainted = DateTime.Now;
        private void Controller_OnEtwEventCounter(object sender, OM.EtwEventCounterArgs e)
        {
            if (firstEvent == null)
            {
                firstEvent = DateTime.Now;
            }
            TimeSpan timeSinceLastPaint = DateTime.Now - uiLastPainted;

            if (timeSinceLastPaint > uiDelay)
            {
                statusTotalEtwCount = String.Format("Total Events: {0}  ", e.TotalCount.ToString("##,#0"));
                statusFilterEtwCount = String.Format("Filtered Events: {0}  ", e.FilteredCount.ToString("##,#0"));
                statusEtwDelay = String.Format("Delay: {0}ms   ", (Math.Round((DateTime.Now - e.Timestamp).TotalMilliseconds, 0)).ToString("##,#0"));

                uiLastPainted = DateTime.Now;

                UpdateStatusUI();
            }
        }

        private void Controller_OnEtwEvent(object sender, OM.EtwEventArgs e)
        {
            UpdateDataGridUI(e.EtwEvent);
        }

        private void UpdateDataGridUI(EtwEvent e)
        {
            if (lstItems.InvokeRequired)
            {
                lstItems.Invoke(new MethodInvoker(delegate() { UpdateDataGridUI(e); }));
            }
            else
            {
                TimeSpan offset = firstEvent.Value - e.Timestamp;
                string timeOffset = String.Format("+{0}ms", (Math.Abs(offset.TotalMilliseconds)).ToString("##,#0"));
                ListViewItem item = new ListViewItem(timeOffset, 0);
                item.SubItems.Add(e.PID.ToString());
                item.SubItems.Add(e.Name);
                item.SubItems.Add(e.Text);
                lstItems.Items.Add(item);
            }
        }

        private void UpdateStatusUI()
        {
            lblStatusTotalEtwCount.Text = statusTotalEtwCount;
            lblStatusFilterEtwCount.Text = statusFilterEtwCount;
            lblStatusEtwDelay.Text = statusEtwDelay;
        }

        private bool running = false;
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                if (cmbProcesses.SelectedItem != null)
                {
                    try
                    {
                        OM.Controller.Start(((KeyValuePair<int, String>)cmbProcesses.SelectedItem).Key);
                    }
                    catch (Win32Exception ex)
                    {
                        switch(ex.ErrorCode)
                        {
                            case -2147467259:
                                ShowError("Access Denied", "Access denied, please run this tool elevated.");
                                break;
            
                            default:
                                ShowError("Unknown Win32 Error", "Unknown Win32 error occurred error code: "+ex.ErrorCode+"Error Message:\n"+ex.Message);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError("Error", "Error subscribing to ETW events for \"" + ((KeyValuePair<int, String>)cmbProcesses.SelectedItem).Value + "\". \nError Message:\n" + ex.Message);
                    }
                }
                else
                {
                    OM.Controller.Start();
                }

                btnRun.Text = "Stop";
                running = true;
            }
            else
            {
                OM.Controller.Stop();
                btnRun.Text = "Start";
                running = false;
            }
        }

        private void ShowError(string Caption, string Text)
        {
            MessageBox.Show(Text, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings settings = new frmSettings();
            settings.Show();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
