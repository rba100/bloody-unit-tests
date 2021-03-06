﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using BloodyUnitTests.Engine;

namespace BloodyUnitTests
{
    public partial class WriteAllTestsDialog : Form
    {
        public Assembly TargetAssembly { get; }

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

        private void WriteAllTestsDialog_Load(object sender, EventArgs e)
        {
            new ToolTip().SetToolTip(label2, "Generate tests for classes in this root namespace");
        }
    }
}
