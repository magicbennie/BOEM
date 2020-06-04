using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOpsErrorMonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Setup the global exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            Application.ThreadException += new ThreadExceptionEventHandler(UnhandledThreadExceptionHandler);

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            ExceptionHandler(sender, e);
        }

        public static void UnhandledThreadExceptionHandler(object sender, ThreadExceptionEventArgs args)
        {
            Exception e = (Exception)args.Exception;

            ExceptionHandler(sender, e);
        }

        private static void ExceptionHandler(object Sender, Exception e)
        {
            StreamWriter CrashLog;

            string CrashLogName = "BOEMCrash_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

            try
            {
                CrashLog = new StreamWriter(CrashLogName);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Oh noes!\n\nThere was a catastrphic error, from which we can't recover :(\n\nOn top of that, the attempt to write the crashlog failed with error: " + Ex.Message);

                return;
            }

            //Write log
            CrashLog.WriteLine("Black Ops Error Monitor v" + Application.ProductVersion);
            CrashLog.WriteLine("Crash occured on: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            CrashLog.WriteLine("Exception: " + e.Message);
            CrashLog.WriteLine("Exception Source: " + e.Source);
            CrashLog.WriteLine("Stacktrace: " + e.StackTrace);
            CrashLog.WriteLine("Data: " + e.Data);

            CrashLog.Dispose();

            MessageBox.Show("Oh noes!\n\nThere was a catastrphic error, from which we can't recover :(\n\nA crash log (\"" + CrashLogName + "\") has been saved, please forward this to magicbennie.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
