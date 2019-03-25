using System;
using System.Reflection;
using System.Windows.Forms;

namespace BloodyUnitTests
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MainForm();

            if(args.Length == 1) form.LoadAssembly(args[0]);
            else if (args.Length == 2)
            {
                form.LoadAssembly(args[0]);
                form.SelectClass(args[1]);
            }
            else form.LoadAssembly(Assembly.GetEntryAssembly().Location);

            Application.Run(form);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
