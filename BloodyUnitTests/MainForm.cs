
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using ScintillaNET;

using static System.Linq.Enumerable;

namespace BloodyUnitTests
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            SetStatus(string.Empty);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new ToolTip().SetToolTip(btLoadAssembly, "Drag and drop assemblies or source files from VS");
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
            scintilla.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms");
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
                var typeList = m_Assembly.GetTestableClassTypes().Select(c => new TypeRef(c)).ToList();
                cbClassList.DataSource = typeList;
                if(typeList.Any()) cbClassList.SelectedIndex = 0;
                btTestClass.Enabled = true;
                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
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
            CreateTestsForSelectedType();
        }

        private void CreateTestsForSelectedType()
        {
            try
            {
                var classType = cbClassList.SelectedItem as TypeRef;

                var existingTab = m_TabContainer.Controls.OfType<TabPage>()
                                                .FirstOrDefault(p => p.Text == classType.ToString());
                if (existingTab != null)
                {
                    m_TabContainer.SelectedTab = existingTab;
                    return;
                }

                var type = classType.Type;

                var editor = GetDefaultEditor();
                editor.Text = TestFixtureCreator.CreateTestFixture(type) ?? "";

                var tab = new TabPage(classType.ToString());
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
            string filePath = null;

            // Handle file drop (i.e. from Explorer)
            if (e.Data.GetData(DataFormats.FileDrop) is string[] data
                && data.Length == 1
                && File.Exists(data[0]))
            {
                filePath = data[0];
            }
            // Handle string drop (i.e. from VS)
            else if (e.Data.GetData(DataFormats.Text) is string path
                     && File.Exists(path))
            {
                filePath = path;
            }

            if (filePath == null) return;

            var filePathLower = filePath.ToLower();

            if (filePathLower.EndsWith(".exe") || filePathLower.EndsWith(".dll")) LoadAssembly(filePath);
            else if (filePathLower.EndsWith(".cs"))
            {
                var ns        = GetNameSpaceFromCsFile(filePath);
                var className = GetClassNameFromCsFile(filePath);
                if (ns == null || className == null) return;
                var assemblies = GetBinariesNearFolder(Path.GetDirectoryName(filePath), ns).ToArray();
                var best = assemblies.OrderByDescending(a => a.score) // Most matching parts
                                     .ThenBy(a => Path.GetFileName(a.filePath ?? "").Length)  // Shortest file name
                                     .Select(ToNullable)
                                     .FirstOrDefault();
                if (!best.HasValue)
                {
                    SetStatus(@"Unable to find an assembly built from that source file.");
                    return;
                }
                LoadAssembly(best.Value.filePath);
                var match = cbClassList.Items.OfType<TypeRef>().FirstOrDefault(t => t.Type.Name == className);
                if (match == null) SetStatus(@"The requested type could not be loaded for some reason.");
                var itemIndex = cbClassList.Items.IndexOf(match);
                cbClassList.SelectedIndex = itemIndex;
                CreateTestsForSelectedType();
            }

        }

        private static T? ToNullable<T>(T item) where T : struct { return item; }

        private IEnumerable<(int score, string filePath)> GetBinariesNearFolder(string folderPath,
                                                                                string[] nameSpaceParts)
        {
            string[] binaries = null;
            var folder = folderPath;
            const int maxDepth = 4;
            foreach (var _ in Range(1,maxDepth))
            {
                binaries = GetAllBinaries(folder).ToArray();
                if (binaries.Any()) break;
                folder = Directory.GetParent(folder).FullName;
            }

            if (binaries?.Any() == true)
            {
                foreach (var filePath in binaries)
                {
                    var fileName = Path.GetFileName(filePath);
                    var score = GetScore(fileName, nameSpaceParts);
                    if (score == 0) continue;
                    yield return (score, filePath);
                }
            }
        }

        private int GetScore(string fileName, string[] nameSpaceParts)
        {
            int score = 0, curIndex = 0;
            foreach (var part in nameSpaceParts)
            {
                curIndex = fileName.Substring(curIndex).IndexOf(part, StringComparison.Ordinal);
                if (curIndex == -1) break;
                score++;
            }
            return score;
        }

        private IEnumerable<string> GetAllBinaries(string rootDir)
        {
            var dlls = Directory.GetFiles(rootDir).Where(f =>
            {
                var l = f.ToLower();
                return l.EndsWith("dll") || l.EndsWith("exe");
            });
            var subFolders = Directory.GetDirectories(rootDir);
            return dlls.Concat(subFolders.SelectMany(GetAllBinaries));
        }

        private string[] GetNameSpaceFromCsFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var nsDeclaration = lines.First(l => l.Contains("namespace"));
                return nsDeclaration.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                    .Split('.');
            }
            catch
            {
                return null;
            }
        }

        private string GetClassNameFromCsFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var classDeclr = lines.First(l => l.Contains(" class "));
                var parts = classDeclr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var index = Array.IndexOf(parts, "class");
                return parts[index + 1];
            }
            catch
            {
                return null;
            }
        }

        private void btLoadAssembly_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void SetStatus(string status)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetStatus(status))); }
            else { statusLabel.Text = status; }
        }
    }

    public class TypeRef
    {
        public Type Type { get; private set; }

        public TypeRef(Type type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.Name;
        }
    }
}