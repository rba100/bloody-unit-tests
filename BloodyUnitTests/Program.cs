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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MainForm();

            if(args.Length == 1) form.LoadAssembly(args[0]);
            else form.LoadAssembly(Assembly.GetEntryAssembly().Location);

            Application.Run(form);
        }
    }
}
