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
            this.m_TabContainer = new System.Windows.Forms.TabControl();
            this.m_Do1 = new System.Windows.Forms.Button();
            this.m_NullTestButton = new System.Windows.Forms.Button();
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
            // m_Do1
            // 
            this.m_Do1.Location = new System.Drawing.Point(12, 12);
            this.m_Do1.Name = "m_Do1";
            this.m_Do1.Size = new System.Drawing.Size(105, 23);
            this.m_Do1.TabIndex = 1;
            this.m_Do1.Text = "Load Assembly";
            this.m_Do1.UseVisualStyleBackColor = true;
            this.m_Do1.Click += new System.EventHandler(this._loadAssembly_click);
            // 
            // m_NullTestButton
            // 
            this.m_NullTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_NullTestButton.Enabled = false;
            this.m_NullTestButton.Location = new System.Drawing.Point(687, 14);
            this.m_NullTestButton.Name = "m_NullTestButton";
            this.m_NullTestButton.Size = new System.Drawing.Size(101, 23);
            this.m_NullTestButton.TabIndex = 1;
            this.m_NullTestButton.Text = "Create tests";
            this.m_NullTestButton.UseVisualStyleBackColor = true;
            this.m_NullTestButton.Click += new System.EventHandler(this._nullTests_click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(123, 14);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(558, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.m_NullTestButton);
            this.Controls.Add(this.m_Do1);
            this.Controls.Add(this.m_TabContainer);
            this.Name = "MainForm";
            this.Text = "Bloody Unit Tests";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl m_TabContainer;
        private System.Windows.Forms.Button m_Do1;
        private System.Windows.Forms.Button m_NullTestButton;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

