using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BloodyUnitTests
{
    public partial class WriteAllTestsDialog : Form
    {
        public Assembly TargetAssembly { get; set; }

        public WriteAllTestsDialog(Assembly targetAssembly) : this()
        {
            TargetAssembly = targetAssembly;
        }

        public WriteAllTestsDialog()
        {
            InitializeComponent();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btChooseFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    tbOutputFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            try
            {
                var projectWriter = new ProjectWriter();
                projectWriter.WriteAllTests(TargetAssembly, tbNameSpace.Text, tbOutputFolder.Text);
                Close();
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
