
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using ScintillaNET;

namespace BloodyUnitTests
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void SetDefaults(Scintilla scintilla)
        {
            scintilla.Dock = DockStyle.Fill;
            // Configure the default style
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.Styles[Style.Default].BackColor = IntToColor(0x212121);
            scintilla.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            scintilla.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            scintilla.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            scintilla.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            scintilla.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            scintilla.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            scintilla.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            scintilla.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            scintilla.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            scintilla.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            scintilla.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            scintilla.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            scintilla.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            scintilla.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            scintilla.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            scintilla.Lexer = Lexer.Cpp;

            scintilla.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            scintilla.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms scintilla");
        }

        private Scintilla GetDefaultEditor()
        {
            var editor = new Scintilla { Dock = DockStyle.Fill };
            SetDefaults(editor);
            return editor;
        }

        private static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        public void LoadAssembly(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;
                m_Assembly = Assembly.LoadFrom(filePath);
                comboBox1.DataSource = m_Assembly.GetTestableClassTypes().Select(c => c.Name).ToList();
                comboBox1.SelectedIndex = 0;
                btTestClass.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btLoadAssembly_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = @"Assemblies|*.dll;*.exe|All files|*.*";
                    var result = dialog.ShowDialog(this);
                    if (result != DialogResult.OK) return;
                    LoadAssembly(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btTestClass_Click(object sender, EventArgs e)
        {
            try
            {
                var type = m_Assembly.GetLoadableTypes().First(t => t.Name == comboBox1.SelectedItem as string);

                var editor = GetDefaultEditor();
                editor.Text = TestFixtureCreator.CreateTestFixture(type) ?? "This class has nothing!";

                var tab = new TabPage(comboBox1.SelectedItem as string);
                tab.Controls.Add(editor);

                m_TabContainer.Controls.Add(tab);
                m_TabContainer.SelectedTab = tab;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btTestAssembly_Click(object sender, EventArgs e)
        {
            using (var testAllForm = new WriteAllTestsDialog(m_Assembly))
            {
                testAllForm.ShowDialog(this);
            }
        }

        private Assembly m_Assembly;

        private void btLoadAssembly_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] data
                && data.Length == 1
                && File.Exists(data[0]))
            {
                LoadAssembly(data[0]);
            }
        }

        private void btLoadAssembly_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }
}
