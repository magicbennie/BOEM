using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOpsErrorMonitor
{
    public partial class frmEntityList : Form
    {
        public frmEntityList()
        {
            InitializeComponent();
        }

        private delegate void UpdateButtonCallback(Button button, bool Enabled);

        private void UpdateButton(Button button, bool Enabled)
        {
            if (Enabled == button.Enabled)
            {
                return;
            }

            if (button.InvokeRequired)
            {
                UpdateButtonCallback Callback = new UpdateButtonCallback(UpdateButton);
                this.Invoke(Callback, new object[] { button, Enabled });
            }
            else
            {
                button.Enabled = Enabled;
            }
        }

        private delegate void UpdateLabelCallback(Label label, string Text);

        private void UpdateLabel(Label label, string Text)
        {
            if (Text == label.Text)
            {
                return;
            }

            if (label.InvokeRequired)
            {
                UpdateLabelCallback Callback = new UpdateLabelCallback(UpdateLabel);
                this.Invoke(Callback, new object[] { label, Text });
            }
            else
            {
                label.Text = Text;
            }
        }

        Thread DataRetreiver;

        //string PingTracker = "";

        //private void PingStatus(string Status)
        //{
        //    PingTracker += Status + Environment.NewLine;
        //}

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            //PingTracker = "";

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Starting Refresh");

            Process BlackOps = Process.GetProcessesByName("BlackOps").FirstOrDefault();

            if (BlackOps == null)
            {
                ShowError("Game not running!");

                lblStatus.Text = "Status: Waiting...";

                return;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got C# handle to game for alive check in btnRefresh_Click");

            btnRefresh.Enabled = false;

            BlackOps.Dispose();

            ClearTable();

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] ClearTable() done, starting new thread...");

            DataRetreiver = new Thread(RefreshData);

            DataRetreiver.Start();

            btnExit.Refresh();
        }

        private void RefreshData()
        {
            try
            {
                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] New thread started!");

                Process BlackOps = Process.GetProcessesByName("BlackOps").FirstOrDefault();

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got C# handle to game for whole function");

                if (BlackOps == null || BlackOps.HasExited)
                {
                    ShowError("Game not running!");

                    UpdateButton(btnRefresh, true);

                    UpdateLabel(lblStatus, "Status: Waiting...");

                    return;
                }

                int BlackOpsHandle = (int)BlackOps.SafeHandle.DangerousGetHandle();//MagicMemory.GetProcessHandle("BlackOps.exe");

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got unmanaged handle");

                if (BlackOpsHandle == 0)
                {
                    ShowError("Unable to interact with the game!");

                    UpdateButton(btnRefresh, true);

                    UpdateLabel(lblStatus, "Status: Waiting...");

                    return;
                }

                UpdateLabel(lblStatus, "Status: Working...");

                string Data = GetEntities(BlackOps);

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] GetEntities done!");

                if (string.IsNullOrWhiteSpace(Data))
                {
                    ShowError("Unable to get list of entities!");

                    UpdateButton(btnRefresh, true);

                    UpdateLabel(lblStatus, "Status: Failed!");

                    return;
                }

                //MessageBox.Show("Length: " + Data.Length, "Debug");

                int Counter = 0;

                string[] Entities = Data.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string CurrentEntity in Entities)
                {
                    string[] EntityData = CurrentEntity.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                    if (EntityData.Length != 6)
                    {
                        ShowError("Corrupt data in entity!");

                        continue;
                    }

                    Counter++;

                    AddRow(EntityData[0], EntityData[1], EntityData[2], EntityData[3], EntityData[4], EntityData[5]);

                    //dgvTemp.Rows.Add(EntityData[0], EntityData[1], EntityData[2], EntityData[3], EntityData[4], EntityData[5]);

                    //AddRow(Convert.ToInt32(EntityData[0]), EntityData[1], EntityData[2], EntityData[3], EntityData[4], EntityData[5]);
                    //dgvEntities.Rows.Insert(EntityID, Type, Class, Name, Target, Model);
                }

                UpdateLabel(lblStatus, "Status: Done (" + Counter + " entities displayed)");

                UpdateButton(btnRefresh, true);

                //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Finished!");

                //MessageBox.Show(PingTracker, "Debug");

                BlackOps.Dispose();

                if (btnExit.Text == "Exiting...")
                {
                    this.BeginInvoke(new MethodInvoker(this.Close));
                }
            }
            catch (Exception Ex)
            {
                UpdateLabel(lblStatus, "Status: Failed!");

                UpdateButton(btnRefresh, true);

                ShowError("There was an error!\n\nDetails: " + Ex.Message);
            }
        }

        private delegate void AddRowCallback(string EntityID, string Type, string Class, string Name, string Target, string Model);

        private void AddRow(string EntityID, string Type, string Class, string Name, string Target, string Model)
        {
            if (dgvEntities.InvokeRequired)
            {
                AddRowCallback Callback = new AddRowCallback(AddRow);
                this.Invoke(Callback, new object[] { EntityID, Type, Class, Name, Target, Model });
            }
            else
            {
                dgvEntities.Rows.Add(Type, Class, Name, Target, Model);
            }
        }

        private void ShowError(string ErrorMessage)
        {
            MessageBox.Show("Error: " + ErrorMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetEntities(Process BlackOps)
        {
            //Get handle to or inject DLL
            HookManager HM = new HookManager();

            int ModuleHandle = HM.GetHookDllHandle(BlackOps);

            if (ModuleHandle == 0)
            {
                MessageBox.Show("There was an error managing the hook DLL!" + (string.IsNullOrEmpty(HM.LastError) ? "" : ("\n\nError details: " + HM.LastError)));

                return null;
            }

            IntPtr BlackOpsHandle = BlackOps.SafeHandle.DangerousGetHandle();

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Finished attempt to get handle to MagicbennieBO1InternalHooks.dll");
            
            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] About to get GetEntities address");

            MM_RESULT GetEntitiesResult = MagicMemory.GetProcessAddress(BlackOpsHandle, ModuleHandle, "GetEntities");

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got Proc address for GetEntities");

            if (!GetEntitiesResult.Success || GetEntitiesResult.Int32Result == 0)
            {
                ShowError("Failed to get location of GetEntities in module MagicbennieBO1InternalHooks.dll!");

                return null;
            }

            int GetEntities = GetEntitiesResult.Int32Result;

            int ThreadHandle = MagicMemory.CreateARemoteThread(BlackOpsHandle, GetEntities, 0);

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Created remote thread, waiting for it...");

            if (ThreadHandle == 0)
            {
                ShowError("Failed to start remote thread at GetEntities!");

                return null;
            }

            if (!MagicMemory.WaitForThread(ThreadHandle, 20000))
            {
                ShowError("Failed to wait for thread!");

                return null;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Thread done! Reading string...");

            int DataLoc = MagicMemory.GetThreadExitCode(ThreadHandle);

            if (DataLoc == 0)
            {
                ShowError("GetEntities failed!");

                return null;
            }

            //Close handle
            MagicMemory.CloseObjectHandle(ThreadHandle);

            string Data = MagicMemory.ReadString(BlackOpsHandle, DataLoc);

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Done reading string, freeing memory...");

            //MessageBox.Show("Count: " + Data.Length);

            MagicMemory.UnAllocateMemory(BlackOpsHandle, DataLoc);

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Done freeing, returning data!");

            return Data;
        }

        private void ClearTable()
        {
            dgvEntities.Rows.Clear();
            dgvEntities.Columns.Clear();

            //[804] Type: , Class: , Name: zombie, Target: None, Model: None
            dgvEntities.Columns.Add("Type", "Type");
            dgvEntities.Columns.Add("Class", "Class");
            dgvEntities.Columns.Add("Name", "Name");
            dgvEntities.Columns.Add("Target", "Target");
            dgvEntities.Columns.Add("Model", "Model");

            dgvEntities.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//dgvEntities.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.Fill);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text == "Status: Working...")
            {
                btnExit.Enabled = false;
                btnExit.Text = "Exiting...";
                return;
            }

            this.Close();
        }

        private void frmEntityList_Load(object sender, EventArgs e)
        {
            ClearTable();
        }

        private void frmEntityList_Resize(object sender, EventArgs e)
        {
            //MessageBox.Show("Height: " + this.Size.Height + ", Width: " + this.Size.Width);

            if (this.Height < 200 || this.Width < 300)
            {
                this.Height = 200;
                this.Width = 300;
            }

            //DGV SIZE: 719, 536
            //FRM SIZE: 759, 625
            //LBL SIZE: ? LOC: 12, 559

            dgvEntities.Size = new Size(this.Size.Width - 40, this.Size.Height - 85);
            btnExit.Location = new Point((this.Size.Width - btnExit.Size.Width) - 27, (this.Size.Height - btnExit.Size.Height) - 45);
            btnRefresh.Location = new Point((this.Size.Width - btnRefresh.Size.Width - btnExit.Size.Width) - 33, (this.Size.Height - btnExit.Size.Height) - 45);
            lblStatus.Location = new Point(12, (this.Size.Height - lblStatus.Size.Height) - 48);
        }
    }
}
