using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Reflection;
using BlackOpsErrorMonitor.Properties;

namespace BlackOpsErrorMonitor
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private bool DoExit = false;
        private bool ShouldExitThread = false;

        private Thread GameScannerThread;
        private ConfigManager Config;

        //What we are monitoring
        private const int LevelTimeAddress = 0x0286D014; //Max: 1879048192

        private const int numGEntitiesAllocatedAddress = 0x01C0314C; //Max: 1022

        private const int numSnapshotEntitiesAddress_1 = 0x0286D034; //Max: 2147483646
        private const int numSnapshotEntitiesAddress_2 = 0x0286D024;

        private const int nextCachedSnapshotEntitiesAddress = 0x0286D09C; //Max: 2147479550
        private const int nextCachedSnapshotClientsAddress = 0x0286D0A0; //Max: 2147483630
        private const int nextCachedSnapshotFramesAddress = 0x0286D0A8; //Max: 2147483134

        private const int numSnapshotClientsAddress_1 = 0x0286D044; //Max: 2147483646
        private const int numSnapshotClientsAddress_2 = 0x0286D028;

        private const int numSnapshotActorsAddress_1 = 0x0286D054; //Max: 2147483646
        private const int numSnapshotActorsAddress_2 = 0x0286D02C;

        private const int ZombiesInMapAddress = 0x01BFBC20;

        private const int LastNetSnapEntitiesAddress = 0x02911CB8; //cl->snap.numEntities;

        private int LastDetection10snumSnapshotEntitesValue = 0;
        private int LastDetection10sValue = 0;
        private long LastDetection10sTimeStamp = 0;
        private int LastDetection30snumSnapshotEntitesValue = 0;
        private int LastDetection30sValue = 0;
        private long LastDetection30sTimeStamp = 0;
        private int LastDetection1mnumSnapshotEntitesValue = 0;
        private int LastDetection1mValue = 0;
        private long LastDetection1mTimeStamp = 0;

        public delegate void AttemptExitCallback();

        void AttemptExit()
        {
            if (this.InvokeRequired)
            {
                AttemptExitCallback Callback = new AttemptExitCallback(AttemptExit);
                this.Invoke(Callback);
            }
            else
            {
                DoExit = true;

                ShouldExitThread = true;

                if (GameScannerThread != null)
                {
                    GameScannerThread.Abort();

                    int WaitTime = 0;

                    while (GameScannerThread.IsAlive)
                    {
                        if (WaitTime <= 1000)
                        {
                            WaitTime += 100;
                            Thread.Sleep(100);
                        }
                        else
                        {
                            //ShowError("Can't close down game interface, you might need to force close the program.");

                            this.Close();

                            return;
                        }
                    }
                }

                this.Close();
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Black Ops 1 Error Monitor v" + Utils.GetBOEMVersion() + "\n\nBy: magicbennie\n\nThis program was intended for debugging and monitoring purposes only. It does not fix any resets or errors.\n\nFeedback:\n\nIf you would like to report an issue, make a suggestion or otherwise provide feedback about this program, please contact magicbennie on Twitch, Twitter or in the Discord.\n\nWould you like to open the discord now?", "About", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://discord.gg/CBUYEEB");
            }
        }

        private void CheckThread()
        {
            if (GameScannerThread == null || !GameScannerThread.IsAlive)
            {
                ShouldExitThread = false;

                GameScannerThread = new Thread(ThreadProc);
                GameScannerThread.IsBackground = true;
                GameScannerThread.Name = "GameScannerThread";
                GameScannerThread.Start();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            AttemptExit();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!DoExit)
            {
                AttemptExit();
            }
        }

        private delegate void ChangeLabelCallback(Label label, string Text, Color Colour);

        private void ChangeLabel(Label label, string Text, Color Colour)
        {
            if (Text == label.Text && Colour == label.BackColor)
            {
                return;
            }

            if (label.InvokeRequired)
            {
                ChangeLabelCallback Callback = new ChangeLabelCallback(ChangeLabel);
                this.Invoke(Callback, new object[] { label, Text, Colour });
            }
            else
            {
                label.Text = Text;
                label.ForeColor = Colour;
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

        private delegate void UpdateProgressBarCallback(ProgressBar progressBar, int Value);

        private void UpdateProgressBar(ProgressBar progressBar, int Value)
        {
            if (Value == progressBar.Value)
            {
                return;
            }

            if (progressBar.InvokeRequired)
            {
                UpdateProgressBarCallback Callback = new UpdateProgressBarCallback(UpdateProgressBar);
                this.Invoke(Callback, new object[] { progressBar, Value });
            }
            else
            {
                if (Value >= progressBar.Maximum)
                {
                    ModifyProgressBarColor.SetState(progressBar, 2);
                }
                else
                {
                    ModifyProgressBarColor.SetState(progressBar, 1);
                }

                progressBar.Value = Value;
            }
        }

        private delegate void UpdateProgressBarUIntCallback(ProgressBar progressBar, UInt32 Value, UInt32 Maximum);

        private void UpdateProgressBarUInt(ProgressBar progressBar, UInt32 Value, UInt32 Maximum)
        {
            if (progressBar.InvokeRequired)
            {
                UpdateProgressBarUIntCallback Callback = new UpdateProgressBarUIntCallback(UpdateProgressBarUInt);
                this.Invoke(Callback, new object[] { progressBar, Value, Maximum });
            }
            else
            {
                if (progressBar.Maximum != 100)
                {
                    progressBar.Maximum = 100;
                }

                decimal Result = ((decimal)Value / (decimal)Maximum) * 100;

                int PercentageToDisplay = Convert.ToInt32(Result);

                if (PercentageToDisplay > progressBar.Maximum)
                    PercentageToDisplay = progressBar.Maximum;

                progressBar.Value = PercentageToDisplay;

                if (Value >= Maximum)
                {
                    ModifyProgressBarColor.SetState(progressBar, 2);
                }
                else
                {
                    ModifyProgressBarColor.SetState(progressBar, 1);
                }
            }
        }

        private delegate void UpdateProgressBar2Callback(ProgressBar progressBar, int Value, int MaxValue);

        private void UpdateProgressBar(ProgressBar progressBar, int Value, int MaxValue)
        {
            if (Value == progressBar.Value && MaxValue == progressBar.Maximum)
            {
                return;
            }

            if (progressBar.InvokeRequired)
            {
                UpdateProgressBar2Callback Callback = new UpdateProgressBar2Callback(UpdateProgressBar);
                this.Invoke(Callback, new object[] { progressBar, Value, MaxValue });
            }
            else
            {
                progressBar.Maximum = MaxValue;

                if (Value >= progressBar.Maximum)
                {
                    Value = MaxValue;
                    ModifyProgressBarColor.SetState(progressBar, 2);
                }
                else
                {
                    ModifyProgressBarColor.SetState(progressBar, 1);
                }

                progressBar.Value = Value;
            }
        }

        private void ThreadProc()
        {
            IntPtr BlackOps = IntPtr.Zero;
            int BlackOpsPID = 0;
            Process BlackOpsProc = null;
            int Time;

            while (!ShouldExitThread)
            {
                try
                {
                    if (MagicMemory.GetProcessExists("BlackOps"))
                    {
                        if (BlackOps == IntPtr.Zero || BlackOpsPID != MagicMemory.GetProcessID("BlackOps"))
                        {
                            BlackOps = MagicMemory.GetProcessHandle("BlackOps");
                            BlackOpsPID = MagicMemory.GetProcessID("BlackOps");
                            BlackOpsProc = Process.GetProcessById(BlackOpsPID);

                            if (BlackOps == IntPtr.Zero)
                            {
                                //abort!
                                ShouldExitThread = true;

                                SEHWasApplied = false; 

                                ChangeLabel(lblGameStatus, "Game Not Connected!", Color.Red);

                                //ShowError("Can't get a handle to Black Ops!\n\nDo you have the correct permissions?\n\nClick Enable/Disable to try again.");

                                continue;
                            }
                            else
                            {
                                ChangeLabel(lblGameStatus, "Game Connected!", Color.Green);

                                SetFormState(true);

                                //Clear old timestamp values
                                LastDetection10snumSnapshotEntitesValue = 0;
                                LastDetection10sValue = 0;
                                LastDetection10sTimeStamp = 0;
                                LastDetection30snumSnapshotEntitesValue = 0;
                                LastDetection30sValue = 0;
                                LastDetection30sTimeStamp = 0;
                                LastDetection1mnumSnapshotEntitesValue = 0;
                                LastDetection1mValue = 0;
                                LastDetection1mTimeStamp = 0;

                                //Hook here
                                if (Config.ParseStringAsBoolean(Config.GetSetting("OverrideSEH"))) 
                                {
                                    EnableSEHOverride(BlackOpsProc);
                                }
                            }
                        }

                        //LevelTime
                        int LevelTime = MagicMemory.ReadInt(BlackOps, LevelTimeAddress);

                        //Check for error
                        if (LevelTime == 0)
                        {
                            //Might be because the game isn't ready?

                            //ShowError("LevelTime is zero!");

                            Thread.Sleep(1000); //Give it a second and try again

                            //ShouldExitThread = true;

                            //ChangelblStatus("Can't interact with game!", Color.Red);

                            //ShowError("Can't read Black Ops' memory!\n\nDo you have the correct permissions?");

                            continue;
                        }

                        //LastNetSnapEntities
                        int LastNetSnapEntities = MagicMemory.ReadInt(BlackOps, LastNetSnapEntitiesAddress);

                        UpdateLabel(lblLastNetSnapEntities, LastNetSnapEntities.ToString());

                        //SEH Hook
                        bool OverrideSEH = Config.ParseStringAsBoolean(Config.GetSetting("OverrideSEH")); 

                        if (OverrideSEH && !SEHWasApplied)
                        {
                            EnableSEHOverride(BlackOpsProc);
                        }
                        else if (!OverrideSEH && SEHWasApplied)
                        {
                            DisableSEHOverride(BlackOpsProc);
                        }

                        TimeSpan GameTime = TimeSpan.FromMilliseconds(LevelTime);

                        string TimeInGame = string.Format("{0} Hours, {1} Minutes, {2} Seconds", (GameTime.Hours + (GameTime.Days * 24)), GameTime.Minutes, GameTime.Seconds);

                        UpdateProgressBar(prgLevelTimeBar, LevelTime);
                        UpdateLabel(lblLevelTimeBar, LevelTime + " / 1879048192");
                        UpdateLabel(lblLevelTime, LevelTime + " / 1879048192");
                        UpdateLabel(lblLevelTimeTime, TimeInGame);

                        //processMemory
                        if (BlackOpsProc != null)
                        {
                            UInt32 MemoryUsage = Convert.ToUInt32(BlackOpsProc.VirtualMemorySize64); //Idk if this is the right value tbh

                            double Progress = (((double)MemoryUsage / (double)4294967295) * 100);
                            UpdateProgressBar(prgMemoryUsage, Convert.ToInt32((MemoryUsage / 1000000)));
                            UpdateLabel(lblMemoryUsage, (MemoryUsage / 1000000) + " MB / " + (4294967295 / 1000000) + " MB");
                        }

                        //numGEntitiesAllocated
                        int numGEntitiesAllocated = MagicMemory.ReadInt(BlackOps, numGEntitiesAllocatedAddress);

                        UpdateProgressBar(prgnumGEntitiesAllocated, numGEntitiesAllocated);
                        UpdateLabel(lblnumGEntitiesAllocated, numGEntitiesAllocated + " / 1022");

                        //numGEntitiesActual
                        int CurrentEntity = 0x01A7981C;
                        int EntityCounter = 0;
                        int LiveEntities = 0;

                        do
                        {
                            if (MagicMemory.ReadInt(BlackOps, CurrentEntity - 71) != 0)
                            {
                                LiveEntities++;
                            }

                            CurrentEntity += 844;
                            EntityCounter++;
                        } while (EntityCounter < numGEntitiesAllocated);

                        UpdateProgressBar(prgnumGEntitiesUsed, LiveEntities, numGEntitiesAllocated);
                        UpdateLabel(lblnumGEntitiesUsed, LiveEntities + " / " + numGEntitiesAllocated);

                        int numSnapshotEntities_2 = MagicMemory.ReadInt(BlackOps, numSnapshotEntitiesAddress_2);
                        int numSnapshotEntitiesP1 = MagicMemory.ReadInt(BlackOps, numSnapshotEntitiesAddress_1);
                        int numSnapshotEntitiesP2 = MagicMemory.ReadInt(BlackOps, numSnapshotEntitiesAddress_1 + 4);
                        int numSnapshotEntitiesP3 = MagicMemory.ReadInt(BlackOps, numSnapshotEntitiesAddress_1 + 8);
                        int numSnapshotEntitiesP4 = MagicMemory.ReadInt(BlackOps, numSnapshotEntitiesAddress_1 + 12);
                        int maxSnapshotEntities = 2147483646 - numSnapshotEntities_2;

                        //Player 1
                        UpdateProgressBar(prgnumSnapshotEntitiesP1, numSnapshotEntitiesP1, maxSnapshotEntities);
                        UpdateLabel(lblnumSnapshotEntitiesP1, numSnapshotEntitiesP1 + " / " + maxSnapshotEntities);
                        string PercentageString = "0%";
                        if (numSnapshotEntitiesP1 > 0 && maxSnapshotEntities > 0)
                        {
                            double Percentage = ((double)numSnapshotEntitiesP1 / (double)maxSnapshotEntities) * 100;
                            PercentageString = Math.Round(Percentage, 2) + "%";
                        }
                        UpdateLabel(lblnumSnapshotEntitiesPercentage, PercentageString);
                        long CurrentTimeStamp;
                        try
                        {
                            CurrentTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                        catch
                        {
                            CurrentTimeStamp = Convert.ToInt64((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                        }
                        if (LastDetection10sTimeStamp == 0 || CurrentTimeStamp > LastDetection10sTimeStamp + 10)
                        {
                            LastDetection10sTimeStamp = CurrentTimeStamp;
                            int NewDetection = numSnapshotEntitiesP1 - LastDetection10snumSnapshotEntitesValue;

                            if (LastDetection10snumSnapshotEntitesValue != numSnapshotEntitiesP1)
                            {
                                if (LastDetection10sValue == 0)
                                {
                                    if (NewDetection == 0)
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease10s, NewDetection.ToString());
                                    }
                                    else
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease10s, NewDetection + " (+100%)");
                                    }
                                }
                                else
                                {
                                    decimal Change = CalculateChange(LastDetection10sValue, NewDetection);

                                    UpdateLabel(lblnumSnapshotEntitesIncrease10s, NewDetection + " (" + DecimalToPercentageStringPlusMinus(Change) + ")");
                                }
                            }
                            else
                            {
                                UpdateLabel(lblnumSnapshotEntitesIncrease10s, "0");
                            }

                            LastDetection10snumSnapshotEntitesValue = numSnapshotEntitiesP1;
                            LastDetection10sValue = NewDetection;
                        }
                        if (LastDetection30sTimeStamp == 0 || CurrentTimeStamp > LastDetection30sTimeStamp + 30)
                        {
                            LastDetection30sTimeStamp = CurrentTimeStamp;
                            int NewDetection = numSnapshotEntitiesP1 - LastDetection30snumSnapshotEntitesValue;

                            if (LastDetection30snumSnapshotEntitesValue != numSnapshotEntitiesP1)
                            {
                                if (LastDetection30sValue == 0)
                                {
                                    if (NewDetection == 0)
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease30s, NewDetection.ToString());
                                    }
                                    else
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease30s, NewDetection + " (+100%)");
                                    }
                                }
                                else
                                {
                                    decimal Change = CalculateChange(LastDetection30sValue, NewDetection);

                                    UpdateLabel(lblnumSnapshotEntitesIncrease30s, NewDetection + " (" + DecimalToPercentageStringPlusMinus(Change) + ")");
                                }
                            }
                            else
                            {
                                UpdateLabel(lblnumSnapshotEntitesIncrease30s, "0");
                            }

                            LastDetection30snumSnapshotEntitesValue = numSnapshotEntitiesP1;
                            LastDetection30sValue = NewDetection;
                        }
                        if (LastDetection1mTimeStamp == 0 || CurrentTimeStamp > LastDetection1mTimeStamp + 60)
                        {
                            LastDetection1mTimeStamp = CurrentTimeStamp;
                            int NewDetection = numSnapshotEntitiesP1 - LastDetection1mnumSnapshotEntitesValue;

                            if (LastDetection1mnumSnapshotEntitesValue != numSnapshotEntitiesP1)
                            {
                                if (LastDetection1mValue == 0)
                                {
                                    if (NewDetection == 0)
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease1m, NewDetection.ToString());
                                    }
                                    else
                                    {
                                        UpdateLabel(lblnumSnapshotEntitesIncrease1m, NewDetection + " (+100%)");
                                    }
                                }
                                else
                                {
                                    decimal Change = CalculateChange(LastDetection1mValue, NewDetection);

                                    UpdateLabel(lblnumSnapshotEntitesIncrease1m, NewDetection + " (" + DecimalToPercentageStringPlusMinus(Change) + ")");
                                }
                            }
                            else
                            {
                                UpdateLabel(lblnumSnapshotEntitesIncrease1m, "0");
                            }

                            LastDetection1mnumSnapshotEntitesValue = numSnapshotEntitiesP1;
                            LastDetection1mValue = NewDetection;
                        }

                        //Player 2
                        UpdateProgressBar(prgnumSnapshotEntitiesP2, numSnapshotEntitiesP2, maxSnapshotEntities);
                        UpdateLabel(lblnumSnapshotEntitiesP2, numSnapshotEntitiesP2 + " / " + maxSnapshotEntities);
                        //Player 3
                        UpdateProgressBar(prgnumSnapshotEntitiesP3, numSnapshotEntitiesP3, maxSnapshotEntities);
                        UpdateLabel(lblnumSnapshotEntitiesP3, numSnapshotEntitiesP3 + " / " + maxSnapshotEntities);
                        //Player 4
                        UpdateProgressBar(prgnumSnapshotEntitiesP4, numSnapshotEntitiesP4, maxSnapshotEntities);
                        UpdateLabel(lblnumSnapshotEntitiesP4, numSnapshotEntitiesP4 + " / " + maxSnapshotEntities);

                        //nextCachedSnapshotEntitiesAddress
                        int nextCachedSnapshotEntities = MagicMemory.ReadInt(BlackOps, nextCachedSnapshotEntitiesAddress);

                        UpdateProgressBar(prgnextCachedSnapshotEntities, nextCachedSnapshotEntities);
                        UpdateLabel(lblnextCachedSnapshotEntities, nextCachedSnapshotEntities + " / 2147479550");

                        //nextCachedSnapshotClients
                        int nextCachedSnapshotClients = MagicMemory.ReadInt(BlackOps, nextCachedSnapshotClientsAddress);

                        UpdateProgressBar(prgnextCachedSnapshotClients, nextCachedSnapshotClients);
                        UpdateLabel(lblnextCachedSnapshotClients, nextCachedSnapshotClients + " / 2147483630");

                        //nextCachedSnapshotFramesAddress
                        int nextCachedSnapshotFrames = MagicMemory.ReadInt(BlackOps, nextCachedSnapshotFramesAddress);

                        UpdateProgressBar(prgnextCachedSnapshotFrames, nextCachedSnapshotFrames);
                        UpdateLabel(lblnextCachedSnapshotFrames, nextCachedSnapshotFrames + " / 2147483134");

                        //numSnapshotClients
                        int numSnapshotClients_2 = MagicMemory.ReadInt(BlackOps, numSnapshotClientsAddress_2);
                        int numSnapshotClients = MagicMemory.ReadInt(BlackOps, numSnapshotClientsAddress_1);
                        int maxSnapshotClients = 2147483646 - numSnapshotClients_2;

                        UpdateProgressBar(prgnumSnapshotClients, numSnapshotClients, maxSnapshotClients);
                        UpdateLabel(lblnumSnapshotClients, numSnapshotClients + " / " + maxSnapshotClients);

                        //numSnapshotActors
                        int numSnapshotActors_2 = MagicMemory.ReadInt(BlackOps, numSnapshotActorsAddress_2);
                        int numSnapshotActors = MagicMemory.ReadInt(BlackOps, numSnapshotActorsAddress_1);
                        int maxSnapshotActors = 2147483646 - numSnapshotActors_2;

                        UpdateProgressBar(prgnumSnapshotActors, numSnapshotActors, maxSnapshotActors);
                        UpdateLabel(lblnumSnapshotActors, numSnapshotActors + " / " + maxSnapshotActors);

                        //Com_FrameTime
                        UInt32 com_frameTime = MagicMemory.ReadUInt(BlackOps, 0x02481764);
                        UpdateLabel(lblcomFrameTime, com_frameTime.ToString() + "/2147483648");
                        UpdateProgressBarUInt(prgcomFrameTime, com_frameTime, 2147483648);
                    }
                    else
                    {
                        BlackOps = IntPtr.Zero;
                        BlackOpsPID = 0;

                        SEHWasApplied = false; 

                        ChangeLabel(lblGameStatus, "Game Not Connected!", Color.Red);
                    }

                    Time = Convert.ToInt32(Config.GetSetting("RefreshTime"));

                    if (Time <= 0 || Time > 10000)
                    {
                        Time = 500;
                        Config.SetSetting("RefreshTime", "500");
                    }
                }
                catch
                {
                    continue;
                }

                Thread.Sleep(Time);
            }
        }

        public decimal CalculateChange(decimal previous, decimal current)
        {
            if (previous == 0)
            {
                throw new InvalidOperationException();
            }

            decimal change = current - previous;
            return change / previous;
        }

        public string DecimalToPercentageStringPlusMinus(decimal d)
        {
            decimal hold = Math.Round((d * 100), 2);

            return (hold > 0 ? "+" : "-") + "%" + (hold > 0 ? hold : -hold).ToString();
        }

        private delegate void SetFormStateCallback(bool Enabled);

        private void SetFormState(bool Enabled)
        {
            if (this.InvokeRequired)
            {
                SetFormStateCallback Callback = new SetFormStateCallback(SetFormState);
                this.Invoke(Callback, new object[] { Enabled });
            }
            else
            {
                btnEntityList.Enabled = Enabled;

                prgLevelTimeBar.Enabled = Enabled;
                prgnextCachedSnapshotClients.Enabled = Enabled;
                prgnextCachedSnapshotEntities.Enabled = Enabled;
                prgnextCachedSnapshotFrames.Enabled = Enabled;
                prgnumGEntitiesAllocated.Enabled = Enabled;
                prgnumSnapshotActors.Enabled = Enabled;
                prgnumSnapshotClients.Enabled = Enabled;
                prgnumSnapshotEntitiesP1.Enabled = Enabled;
                prgMemoryUsage.Enabled = Enabled;
            }
        }

        private bool UpdateCheck()
        {
            string Result = "";

            try
            {
                Result = new WebClient().DownloadString("https://api.magicbennie.com/bo1errormonitor.php?&v=" + Utils.GetBOEMVersion());
            }
            catch
            {
                Utils.ShowError("There was an error communicating with the update server, please try again later.");

                return false;
            }

            string Header = "BlackOpsErrorMonitor;;;";

            if (Result.Length < Header.Length || Result.Substring(0, Header.Length) != Header)
            {
                Utils.ShowError("There was an error communicating with the update server, please try again later.");

                return true;
            }

            Result = Result.Substring(Header.Length);

            if (string.IsNullOrEmpty(Result))
            {
                return true;
            }

            string[] Commands = Result.Split(new string[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string CurrentCommand in Commands)
            {
                string[] CommandFrags = CurrentCommand.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);

                if (CommandFrags.Length <= 0)
                {
                    continue;
                }

                if (CommandFrags[0] == "ShowMessage")
                {
                    if (CommandFrags.Length < 3)
                    {
                        continue;
                    }

                    if (CommandFrags.Length == 4)
                    {
                        DialogResult DResult = MessageBox.Show(CommandFrags[1], CommandFrags[2], MessageBoxButtons.OKCancel);

                        if (DResult == DialogResult.OK)
                        {
                            System.Diagnostics.Process.Start(CommandFrags[3]);
                        }
                    }
                    else if (CommandFrags.Length == 3)
                    {
                        MessageBox.Show(CommandFrags[1], CommandFrags[2]);
                    }
                }
                else if (CommandFrags[0] == "Exit")
                {
                    DoExit = true;
                    ShouldExitThread = true;
                    Application.Exit();
                }
            }

            return true;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Hide();

            SetFormState(false);

            try
            {
                Config = new ConfigManager(Application.StartupPath, "BO1EM.cfg");
            }
            catch (Exception Ex)
            {
                MessageBox.Show("There was an error in accessing the config, details:\n\n\"" + Ex.Message + "\"", "Error!");

                AttemptExit();

                return;
            }

            ReloadLayout();

            UpdateCheck();

            CheckThread();
        }

        private void MessageBoxThread(string Message)
        {
            MessageBox.Show(Message, "Message");
        }

        private void ReloadLayout()
        {
            if (Config == null)
            {
                return;
            }

            this.Hide();

            bool Display_LevelTime = Config.ParseStringAsBoolean(Config.GetSetting("Module_LevelTime"));
            bool Display_LevelTimeBar = Config.ParseStringAsBoolean(Config.GetSetting("Module_LevelTimeBar"));
            bool Display_numGEntitiesAllocated = Config.ParseStringAsBoolean(Config.GetSetting("Module_numGEntitiesAllocated"));
            bool Display_numGEntitiesUsed = Config.ParseStringAsBoolean(Config.GetSetting("Module_numGEntitiesUsed"));
            bool Display_LastNetSnapEntities = Config.ParseStringAsBoolean(Config.GetSetting("Module_LastNetSnapEntities"));
            bool Display_numSnapshotEntitiesP1 = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesP1"));
            bool Display_numSnapshotEntitiesPercentage = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesPercentage"));
            bool Display_numSnapshotEntitiesIncreaseP1 = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesIncreaseP1"));
            bool Display_numSnapshotEntitiesP2 = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesP2"));
            bool Display_numSnapshotEntitiesP3 = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesP3"));
            bool Display_numSnapshotEntitiesP4 = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotEntitiesP4"));
            bool Display_nextCachedSnapshotEntities = Config.ParseStringAsBoolean(Config.GetSetting("Module_nextCachedSnapshotEntities"));
            bool Display_nextCachedSnapshotClients = Config.ParseStringAsBoolean(Config.GetSetting("Module_nextCachedSnapshotClients"));
            bool Display_nextCachedSnapshotFrames = Config.ParseStringAsBoolean(Config.GetSetting("Module_nextCachedSnapshotFrames"));
            bool Display_numSnapshotClients = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotClients"));
            bool Display_numSnapshotActors = Config.ParseStringAsBoolean(Config.GetSetting("Module_numSnapshotActors"));
            bool Display_MemoryUsage = Config.ParseStringAsBoolean(Config.GetSetting("Module_MemoryUsage"));
            bool Display_comFrameTime = Config.ParseStringAsBoolean(Config.GetSetting("Module_comFrameTime"));

            int PaddingBetweenItems = 5;

            int AtX = 12;
            int AtY = 13; //Base height from top of form

            //LevelTime
            if (Display_LevelTime)
            {
                grpLevelTime.Visible = true;
                grpLevelTime.Location = new Point(AtX, AtY);
                AtY += grpLevelTime.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpLevelTime.Visible = false;
            }

            //LevelTimeBar
            if (Display_LevelTimeBar)
            {
                grpLevelTimeBar.Visible = true;
                grpLevelTimeBar.Location = new Point(AtX, AtY);
                AtY += grpLevelTimeBar.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpLevelTimeBar.Visible = false;
            }

            //numGEntitiesAllocated
            if (Display_numGEntitiesAllocated)
            {
                grpnumGEntitiesAllocated.Visible = true;
                grpnumGEntitiesAllocated.Location = new Point(AtX, AtY);
                AtY += grpnumGEntitiesAllocated.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumGEntitiesAllocated.Visible = false;
            }

            //numGEntitiesUsed
            if (Display_numGEntitiesUsed)
            {
                grpnumGEntitiesUsed.Visible = true;
                grpnumGEntitiesUsed.Location = new Point(AtX, AtY);
                AtY += grpnumGEntitiesUsed.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumGEntitiesUsed.Visible = false;
            }

            //LastNetSnapEntities
            if (Display_LastNetSnapEntities)
            {
                grpLastNetSnapEntities.Visible = true;
                grpLastNetSnapEntities.Location = new Point(AtX, AtY);
                AtY += grpLastNetSnapEntities.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpLastNetSnapEntities.Visible = false;
            }

            //numSnapshotEntitiesPercentage
            if (Display_numSnapshotEntitiesPercentage)
            {
                grpnumSnapshotEntitiesPercentage.Visible = true;
                grpnumSnapshotEntitiesPercentage.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesPercentage.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesPercentage.Visible = false;
            }

            //numSnapshotEntitiesP1
            if (Display_numSnapshotEntitiesP1)
            {
                grpnumSnapshotEntitiesP1.Visible = true;
                grpnumSnapshotEntitiesP1.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesP1.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesP1.Visible = false;
            }

            //numSnapshotEntitiesIncreaseP1
            if (Display_numSnapshotEntitiesIncreaseP1)
            {
                grpnumSnapshotEntitiesIncreaseP1.Visible = true;
                grpnumSnapshotEntitiesIncreaseP1.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesIncreaseP1.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesIncreaseP1.Visible = false;
            }

            //numSnapshotEntitiesP2
            if (Display_numSnapshotEntitiesP2)
            {
                grpnumSnapshotEntitiesP2.Visible = true;
                grpnumSnapshotEntitiesP2.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesP2.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesP2.Visible = false;
            }

            //numSnapshotEntitiesP3
            if (Display_numSnapshotEntitiesP3)
            {
                grpnumSnapshotEntitiesP3.Visible = true;
                grpnumSnapshotEntitiesP3.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesP3.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesP3.Visible = false;
            }

            //numSnapshotEntitiesP4
            if (Display_numSnapshotEntitiesP4)
            {
                grpnumSnapshotEntitiesP4.Visible = true;
                grpnumSnapshotEntitiesP4.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotEntitiesP4.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotEntitiesP4.Visible = false;
            }

            //nextCachedSnapshotEntities
            if (Display_nextCachedSnapshotEntities)
            {
                grpnextCachedSnapshotEntities.Visible = true;
                grpnextCachedSnapshotEntities.Location = new Point(AtX, AtY);
                AtY += grpnextCachedSnapshotEntities.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnextCachedSnapshotEntities.Visible = false;
            }

            //nextCachedSnapshotClients
            if (Display_nextCachedSnapshotClients)
            {
                grpnextCachedSnapshotClients.Visible = true;
                grpnextCachedSnapshotClients.Location = new Point(AtX, AtY);
                AtY += grpnextCachedSnapshotClients.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnextCachedSnapshotClients.Visible = false;
            }

            //nextCachedSnapshotFrames
            if (Display_nextCachedSnapshotFrames)
            {
                grpnextCachedSnapshotFrames.Visible = true;
                grpnextCachedSnapshotFrames.Location = new Point(AtX, AtY);
                AtY += grpnextCachedSnapshotFrames.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnextCachedSnapshotFrames.Visible = false;
            }

            //numSnapshotClients
            if (Display_numSnapshotClients)
            {
                grpnumSnapshotClients.Visible = true;
                grpnumSnapshotClients.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotClients.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotClients.Visible = false;
            }

            //numSnapshotActors
            if (Display_numSnapshotActors)
            {
                grpnumSnapshotActors.Visible = true;
                grpnumSnapshotActors.Location = new Point(AtX, AtY);
                AtY += grpnumSnapshotActors.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpnumSnapshotActors.Visible = false;
            }

            //MemoryUsage
            if (Display_MemoryUsage)
            {
                grpMemoryUsage.Visible = true;
                grpMemoryUsage.Location = new Point(AtX, AtY);
                AtY += grpMemoryUsage.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpMemoryUsage.Visible = false;
            }

            //Display_comFrameTime
            if (Display_comFrameTime)
            {
                grpcomFrameTime.Visible = true;
                grpcomFrameTime.Location = new Point(AtX, AtY);
                AtY += grpcomFrameTime.Size.Height + PaddingBetweenItems;
            }
            else
            {
                grpcomFrameTime.Visible = false;
            }

            //Maths went horribly wrong down here
            //Footer, add extra spacing first
            AtY += 6;

            lblStatuWordLabel.Location = new Point(AtX, AtY);
            lblGameStatus.Location = new Point(AtX + 43, AtY);
            btnEntityList.Location = new Point(255, AtY - 4); //255, 479

            //Button row, add height of text and space between next row
            AtY += lblGameStatus.Size.Height + (PaddingBetweenItems * 2);
            //56, 484 -> 
            int TempAtX = AtX;
            btnSettingsAndLayout.Location = new Point(TempAtX, AtY); //12, 511
            TempAtX += btnSettingsAndLayout.Size.Width + 6;
            btnAbout.Location = new Point(TempAtX, AtY); //153, 511
            TempAtX += btnAbout.Size.Width + 6;
            btnExit.Location = new Point(TempAtX, AtY); //242, 511

            //Add height of buttons and padding to bottom of form
            AtY += btnExit.Size.Height + 40; 

            this.Size = new Size(378, AtY + 13); //378, 589

            //Colors
            string backColour = Config.GetSetting("BackColour");

            Color BackColour;

            if (!string.IsNullOrEmpty(backColour))
            {
                try
                {
                    BackColour = ColorTranslator.FromHtml(backColour);
                }
                catch
                {
                    //Reset it
                    BackColour = SystemColors.Control;
                    Config.SetSetting("BackColour", Utils.GetRGBFromColor(BackColour));
                }
            }
            else
            {
                BackColour = SystemColors.Control;
                Config.SetSetting("BackColour", Utils.GetRGBFromColor(BackColour));
            }

            this.BackColor = BackColour;

            foreach (Control CurrentControl in this.Controls)
            {
                if (CurrentControl.GetType() == typeof(Label))
                {
                    CurrentControl.BackColor = BackColor;
                }
                else if (CurrentControl.GetType() == typeof(GroupBox))
                {
                    foreach (Control CurrentGroupBoxControl in CurrentControl.Controls)
                    {
                        if (CurrentGroupBoxControl.GetType() == typeof(Label))
                        {
                            CurrentGroupBoxControl.BackColor = BackColor;
                        }
                    }
                }
            }

            string textColour = Config.GetSetting("TextColour");

            Color TextColour;

            if (!string.IsNullOrEmpty(textColour))
            {
                try
                {
                    TextColour = ColorTranslator.FromHtml(textColour);
                }
                catch
                {
                    //Reset it
                    TextColour = SystemColors.ControlText;
                    Config.SetSetting("TextColour", Utils.GetRGBFromColor(TextColour));
                }
            }
            else
            {
                TextColour = SystemColors.ControlText;
                Config.SetSetting("TextColour", Utils.GetRGBFromColor(TextColour));
            }

            foreach (Control CurrentControl in this.Controls)
            {
                if (CurrentControl.GetType() == typeof(GroupBox))
                {
                    foreach (Control CurrentGroupBoxControl in CurrentControl.Controls)
                    {
                        if (CurrentGroupBoxControl.GetType() == typeof(Label))
                        {
                            CurrentGroupBoxControl.ForeColor = TextColour;
                        }
                    }
                }
            }

            this.Show();
        }

        private void btnSettingsAndLayout_Click(object sender, EventArgs e)
        {
            frmSettingsAndLayout SettingsAndLayout = new frmSettingsAndLayout(Config);

            SettingsAndLayout.ShowDialog();

            SettingsAndLayout.Dispose();

            ReloadLayout();
        }

        frmEntityList EntityList;

        private void btnEntityList_Click(object sender, EventArgs e)
        {
            btnEntityList.Enabled = false;

            //if (EntityList != null)
            //{
            //    EntityList.Dispose();
            //}

            EntityList = new frmEntityList();

            EntityList.FormClosed += EntityList_FormClosed;

            EntityList.Show();
        }

        private void EntityList_FormClosed(object sender, FormClosedEventArgs e)
        {
            btnEntityList.Enabled = true;
        }

        private bool SEHWasApplied = false; 

        private void EnableSEHOverride(Process BlackOps)
        {
            //Check to make sure BlackOps handle is valid
            if (BlackOps == null || BlackOps.HasExited)
            {
                return;
            }

            //Get handle to hook dll
            HookManager HM = new HookManager();

            int ModuleHandle = HM.GetHookDllHandle(BlackOps);

            if (ModuleHandle == 0)
            {
                MessageBox.Show("There was an error managing the hook DLL!" + (string.IsNullOrEmpty(HM.LastError) ? "" : ("\n\nError details: " + HM.LastError)));

                return;
            }

            IntPtr BlackOpsHandle = BlackOps.Handle;

            if (BlackOpsHandle == IntPtr.Zero)
            {
                MessageBox.Show("There was an error interacting with the game, could not get a handle to the process.");

                return;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Finished attempt to get handle to MagicbennieBO1InternalHooks.dll");

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] About to get EnableSEHHook address");

            MM_RESULT EnableSEHHookResult = MagicMemory.GetProcessAddress(BlackOpsHandle, ModuleHandle, "EnableSEHHook");

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got Proc address for EnableSEHHook");

            if (!EnableSEHHookResult.Success || EnableSEHHookResult.Int32Result == 0)
            {
                //Prompt.Close();
                Utils.ShowError("Failed to get location of EnableSEHHook in module MagicbennieBO1InternalHooks.dll!");

                return;
            }

            int EnableSEHHook = EnableSEHHookResult.Int32Result;

            int ThreadHandle = MagicMemory.CreateARemoteThread(BlackOpsHandle, EnableSEHHook, 0);

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Created remote thread, waiting for it...");

            if (ThreadHandle == 0)
            {
                //Prompt.Close();
                Utils.ShowError("Failed to start remote thread at EnableSEHHook!");

                return;
            }

            if (!MagicMemory.WaitForThread(ThreadHandle, 20000))
            {
                MagicMemory.CloseObjectHandle(ThreadHandle);
                //Prompt.Close();
                Utils.ShowError("Failed to wait for thread!");

                return;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Thread done! Reading string...");

            int DataLoc = MagicMemory.GetThreadExitCode(ThreadHandle);

            if (DataLoc == 0)
            {
                MagicMemory.CloseObjectHandle(ThreadHandle);
                Utils.ShowError("EnableSEHHook failed! (already applied?)");

                return;
            }

            MagicMemory.CloseObjectHandle(ThreadHandle);

            MagicMemory.UnAllocateMemory(BlackOpsHandle, DataLoc);

            SEHWasApplied = true;
        }

        private void DisableSEHOverride(Process BlackOps, bool ForceDisable = false)
        {
            //Check if SEH was even applied in the first place
            //This way we can avoid injecting the DLL unless we have to
            if (!SEHWasApplied || ForceDisable)
            {
                return;
            }

            //Check to make sure BlackOps handle is valid
            if (BlackOps == null || BlackOps.HasExited)
            {
                return;
            }

            //Get handle to hook dll
            HookManager HM = new HookManager();

            int ModuleHandle = HM.GetHookDllHandle(BlackOps);

            if (ModuleHandle == 0)
            {
                //Prompt.Close();
                Utils.ShowError("There was an error managing the hook DLL!" + (string.IsNullOrEmpty(HM.LastError) ? "" : ("\n\nError details: " + HM.LastError)));

                return;
            }

            IntPtr BlackOpsHandle = BlackOps.Handle;

            if (BlackOpsHandle == IntPtr.Zero)
            {
                //Prompt.Close();
                MessageBox.Show("There was an error interacting with the game, could not get a handle to the process.");

                return;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Finished attempt to get handle to MagicbennieBO1InternalHooks.dll");

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] About to get DisableSEHHook address");

            MM_RESULT DisableSEHHookResult = MagicMemory.GetProcessAddress(BlackOpsHandle, ModuleHandle, "DisableSEHHook");

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Got Proc address for DisableSEHHook");

            if (!DisableSEHHookResult.Success || DisableSEHHookResult.Int32Result == 0)
            {
                //Prompt.Close();
                Utils.ShowError("Failed to get location of EnableSEHHook in module MagicbennieBO1InternalHooks.dll!");

                return;
            }

            int DisableSEHHook = DisableSEHHookResult.Int32Result;

            int ThreadHandle = MagicMemory.CreateARemoteThread(BlackOpsHandle, DisableSEHHook, 0);

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Created remote thread, waiting for it...");

            if (ThreadHandle == 0)
            {
                Utils.ShowError("Failed to start remote thread at DisableSEHHook!");

                return;
            }

            if (!MagicMemory.WaitForThread(ThreadHandle, 20000))
            {
                MagicMemory.CloseObjectHandle(ThreadHandle);
                Utils.ShowError("Failed to wait for thread!");

                return;
            }

            //PingStatus("[" + DateTime.Now.ToLongTimeString() + "] Thread done! Reading string...");

            int DataLoc = MagicMemory.GetThreadExitCode(ThreadHandle);

            if (DataLoc == 0)
            {
                MagicMemory.CloseObjectHandle(ThreadHandle);
                Utils.ShowError("DisableSEHHook failed! (already applied?)");

                return;
            }

            MagicMemory.CloseObjectHandle(ThreadHandle);

            MagicMemory.UnAllocateMemory(BlackOpsHandle, DataLoc);

            SEHWasApplied = false;
        }
    }
}