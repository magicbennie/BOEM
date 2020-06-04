using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOpsErrorMonitor
{
    public partial class frmSettingsAndLayout : Form
    {
        public frmSettingsAndLayout(ConfigManager config)
        {
            Config = config;

            InitializeComponent();
        }

        ConfigManager Config;

        private void frmSettingsAndLayout_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadConfig()
        {
            string refreshTime = Config.GetSetting("RefreshTime");

            int RefreshTime = 500;

            if (!string.IsNullOrEmpty(refreshTime) && int.TryParse(refreshTime, out RefreshTime))
            {
                if (RefreshTime < numRefreshTime.Maximum || RefreshTime > numRefreshTime.Minimum)
                {
                    numRefreshTime.Value = RefreshTime;
                }
            }

            string backColour = Config.GetSetting("BackColour");

            Color BackColour;

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

            lblBackColour.BackColor = BackColour;

            string textColour = Config.GetSetting("TextColour");

            Color TextColour;

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

            lblTextColour.BackColor = TextColour;

            string SEHOverride = Config.GetSetting("OverrideSEH"); 

            if (!string.IsNullOrEmpty(SEHOverride))
            {
                chkOverrideSEH.Checked = Config.ParseStringAsBoolean(SEHOverride);
            }

            for (int i = 0; i < chklstModules.Items.Count; i++)
            {
                chklstModules.SetItemChecked(i, Config.ParseStringAsBoolean(Config.GetSetting("Module_" + (string)chklstModules.Items[i])));
            }
        }

        private void SaveConfig()
        {
            Config.SetSetting("RefreshTime", Convert.ToString(numRefreshTime.Value));
            Config.SetSetting("OverrideSEH", Convert.ToString(chkOverrideSEH.Checked)); 

            Config.SetSetting("BackColour", Utils.GetRGBFromColor(lblBackColour.BackColor));
            Config.SetSetting("TextColour", Utils.GetRGBFromColor(lblTextColour.BackColor));

            for (int i = 0; i < chklstModules.Items.Count; i++)
            {
                Config.SetSetting("Module_" + (string)chklstModules.Items[i], chklstModules.GetItemChecked(i) ? "1" : "0");
            }
        }

        private void btnSaveExit_Click(object sender, EventArgs e)
        {
            SaveConfig();

            this.Close();
        }

        private void chklstModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            int SelectedIndex = chklstModules.SelectedIndex;

            if (SelectedIndex != -1)
            {
                chklstModules.SetItemChecked(SelectedIndex, !chklstModules.GetItemChecked(SelectedIndex));

                //btnConfigureModule.Enabled = true;
            }
            else
            {
                btnConfigureModule.Enabled = false;
            }
        }

        private void ShowError(string ErrorMessage)
        {
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnConfigureModule_Click(object sender, EventArgs e)
        {
            int SelectedIndex = chklstModules.SelectedIndex;

            if (SelectedIndex != -1)
            {
                //MessageBox.Show("Selected Module: " + chklstModules.Items[SelectedIndex], "Debug");
                MessageBox.Show("Not yet implemented.", "Unavailable");
            }
            else
            {
                ShowError("No module selected!");
            }
        }

        private void btnTextColourPick_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            colorDialog.FullOpen = true;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                lblTextColour.BackColor = colorDialog.Color;
            }
        }

        private void btnBackColourPick_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            colorDialog.CustomColors = new int[] { ColorTranslator.ToOle(SystemColors.Control) };
            colorDialog.FullOpen = true;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                lblBackColour.BackColor = colorDialog.Color;
            }
        }

        private void btnHelpSEH_Click(object sender, EventArgs e) 
        {
            MessageBox.Show("This overrides the games SEH Handler, allowing BOEM to handle any homeless exceptions.\n\nBOEM can provide more detailed reports on what happened in the event of a game crash.", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
