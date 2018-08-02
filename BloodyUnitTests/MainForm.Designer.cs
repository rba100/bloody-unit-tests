namespace BloodyUnitTests
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.m_TabContainer = new System.Windows.Forms.TabControl();
            this.btLoadAssembly = new System.Windows.Forms.Button();
            this.btCreateTests = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // m_TabContainer
            // 
            this.m_TabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_TabContainer.Location = new System.Drawing.Point(12, 43);
            this.m_TabContainer.Name = "m_TabContainer";
            this.m_TabContainer.SelectedIndex = 0;
            this.m_TabContainer.Size = new System.Drawing.Size(776, 395);
            this.m_TabContainer.TabIndex = 0;
            // 
            // btLoadAssembly
            // 
            this.btLoadAssembly.AllowDrop = true;
            this.btLoadAssembly.Location = new System.Drawing.Point(12, 12);
            this.btLoadAssembly.Name = "btLoadAssembly";
            this.btLoadAssembly.Size = new System.Drawing.Size(105, 23);
            this.btLoadAssembly.TabIndex = 1;
            this.btLoadAssembly.Text = "Load Assembly";
            this.btLoadAssembly.UseVisualStyleBackColor = true;
            this.btLoadAssembly.Click += new System.EventHandler(this.btLoadAssembly_Click);
            this.btLoadAssembly.DragDrop += new System.Windows.Forms.DragEventHandler(this.btLoadAssembly_DragDrop);
            this.btLoadAssembly.DragEnter += new System.Windows.Forms.DragEventHandler(this.btLoadAssembly_DragEnter);
            // 
            // btCreateTests
            // 
            this.btCreateTests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreateTests.Enabled = false;
            this.btCreateTests.Location = new System.Drawing.Point(687, 12);
            this.btCreateTests.Name = "btCreateTests";
            this.btCreateTests.Size = new System.Drawing.Size(101, 23);
            this.btCreateTests.TabIndex = 1;
            this.btCreateTests.Text = "Create tests";
            this.btCreateTests.UseVisualStyleBackColor = true;
            this.btCreateTests.Click += new System.EventHandler(this.btCreateTests_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(123, 13);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(558, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btCreateTests);
            this.Controls.Add(this.btLoadAssembly);
            this.Controls.Add(this.m_TabContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Bloody Unit Tests";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl m_TabContainer;
        private System.Windows.Forms.Button btLoadAssembly;
        private System.Windows.Forms.Button btCreateTests;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

