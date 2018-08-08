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
            this.btTestClass = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btTestAssembly = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_TabContainer
            // 
            this.m_TabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_TabContainer.Location = new System.Drawing.Point(12, 42);
            this.m_TabContainer.Name = "m_TabContainer";
            this.m_TabContainer.SelectedIndex = 0;
            this.m_TabContainer.Size = new System.Drawing.Size(776, 396);
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
            // btTestClass
            // 
            this.btTestClass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btTestClass.Enabled = false;
            this.btTestClass.Location = new System.Drawing.Point(612, 12);
            this.btTestClass.Name = "btTestClass";
            this.btTestClass.Size = new System.Drawing.Size(85, 23);
            this.btTestClass.TabIndex = 1;
            this.btTestClass.Text = "Test class";
            this.btTestClass.UseVisualStyleBackColor = true;
            this.btTestClass.Click += new System.EventHandler(this.btTestClass_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(123, 13);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(483, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // btTestAssembly
            // 
            this.btTestAssembly.Location = new System.Drawing.Point(703, 12);
            this.btTestAssembly.Name = "btTestAssembly";
            this.btTestAssembly.Size = new System.Drawing.Size(85, 23);
            this.btTestAssembly.TabIndex = 3;
            this.btTestAssembly.Text = "Test assembly";
            this.btTestAssembly.UseVisualStyleBackColor = true;
            this.btTestAssembly.Click += new System.EventHandler(this.btTestAssembly_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btTestAssembly);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btTestClass);
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
        private System.Windows.Forms.Button btTestClass;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btTestAssembly;
    }
}

