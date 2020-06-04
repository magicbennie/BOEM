using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOpsErrorMonitor
{
    static class Utils
    {
        public static string GetRGBFromColor(Color Colour)
        {
            return "#" + Colour.R.ToString("X2") + Colour.G.ToString("X2") + Colour.B.ToString("X2"); 
        }

        public static void ShowError(string ErrorMessage)
        {
            MessageBox.Show("Error: " + ErrorMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowDebug(string DebugMessage)
        {
#if DEBUG
            MessageBox.Show(DebugMessage, "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
        }

        public static string GetBOEMVersion()
        {
            return Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf('.'));
        }

        public static UInt64 GetUnixTimestamp()
        {
            return (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
