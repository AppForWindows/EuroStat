using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EuroStatApp {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args) {
                DevExpress.XtraEditors.XtraMessageBox.Show((args.ExceptionObject as Exception).Message, (args.ExceptionObject as Exception).Source);
            };
            Application.ThreadException += delegate (Object sender, System.Threading.ThreadExceptionEventArgs args) {
                DevExpress.XtraEditors.XtraMessageBox.Show(args.Exception.Message, args.Exception.Source);
            };


            Application.Run(new EuroStatForm());
        }
    }
}
