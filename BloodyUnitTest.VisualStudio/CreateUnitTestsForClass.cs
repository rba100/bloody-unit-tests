using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BloodyUnitTests.VisualStudio
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateUnitTestsForClass
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("77ed74a1-656e-449f-b898-169d3d8ad188");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUnitTestsForClass"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateUnitTestsForClass(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand myCommand)
            {
                //var files = SelectedFiles();
                //var valid = files.Length == 1 && files.First().ToLower().EndsWith(".cs");
                var file = GetProjectItem();
                var valid = file?.Name.ToLower().EndsWith(".cs") == true;
                myCommand.Text = valid ? "Generate unit tests" : "oh no!";
                myCommand.Visible = myCommand.Enabled = valid;
            }
        }
        public static ProjectItem GetProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;

            var monitorSelection =
                (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            if (Marshal.GetTypedObjectForIUnknown(hierarchyPointer,
                                                  typeof(IVsHierarchy))
                is IVsHierarchy selectedHierarchy)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(projectItemId,
                                                                          (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                                          out selectedObject));
            }
            return selectedObject as ProjectItem;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateUnitTestsForClass Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CreateUnitTestsForClass's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CreateUnitTestsForClass(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var files = SelectedFiles();

            string message = string.Format(CultureInfo.CurrentCulture, string.Join(", ", files), this.GetType().FullName);
            string title = "Generate units tests";

            var dte = (DTE2) this.ServiceProvider.GetServiceAsync(typeof(DTE)).Result;
            var file = dte.ItemOperations.NewFile(@"General\Visual C# Class", "ObjectOne", EnvDTE.Constants.vsViewKindTextView);
            //var textDocument = file.Document.Object("") as TextDocument;
            //textDocument.Selection.SelectAll();
            //textDocument.Selection.Delete();
            //textDocument.Selection.Insert("Hello, World!");
            // Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.package,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private string[] SelectedFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selectedItems = ((UIHierarchy)((DTE2)this.ServiceProvider.GetServiceAsync(typeof(DTE)).Result)
                                              .Windows
                                              .Item("{3AE79031-E1BC-11D0-8F78-00A0C9110057}").Object).SelectedItems as object[];

            if (selectedItems == null)
            {
                return new string[0];
            }

            var files = from t in selectedItems
                where (t as UIHierarchyItem)?.Object is ProjectItem
                select ((ProjectItem)((UIHierarchyItem)t).Object).FileNames[1];
            return files.ToArray();
        }
    }
}
