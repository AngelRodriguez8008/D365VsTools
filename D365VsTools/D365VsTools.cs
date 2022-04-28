using System.Runtime.InteropServices;
using D365VsTools.CodeGenerator;
using D365VsTools.CodeGenerator.Commands;
using D365VsTools.Configuration;
using Microsoft.VisualStudio.Shell;
using D365VsTools.VisualStudio;
using D365VsTools.WebResourceUpdater;
using D365VsTools.WebResourceUpdater.Commands;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace D365VsTools
{
    /// <summary>
    /// D365-VS-Tools extension package class
    /// </summary>
     
    [PackageRegistration(UseManagedResourcesOnly = false, SatellitePath = "")]
    [ProvideBindingPath]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideCodeGenerator(typeof(XrmCodeGenerator), XrmCodeGenerator.Name, XrmCodeGenerator.Description, true)]
    [ProvideUIContextRule("69760bd3-80f0-4901-818d-c4656aaa08e9", // Must match the GUID in the .vsct file
        name: "UI Context",
        expression: "tt", // This will make the button only show on .tt files (Ex: js | css | html)
        termNames: new[] { "tt" },
        termValues: new[] { "HierSingleSelectionName:.tt$" })]
    [Guid(ProjectGuids.Package)]
    public sealed class D365VsTools : Package
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// </summary>
        public D365VsTools()
        {
            ProjectHelper.SetServiceProvider(this);
            Logger.Initialize();
        }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
      
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = GetService(typeof(SDTE)) as EnvDTE80.DTE2;

            var extendedLog = true;
            var settings = ProjectHelper.GetSettings<Settings>();
            if(settings != null)
                extendedLog = settings.ExtendedLog;

            if (dte == null) // The IDE is not yet fully initialized
            {
                Logger.WriteLine("Warning: DTE service is null. Seems that VisualStudio is not fully initialized.", extendedLog);
                Logger.WriteLine("Waiting for DTE.", extendedLog);
            }
            else
            {
                Logger.WriteLine("DTE service found.", extendedLog);
                UpdateSelectedWebResources.Initialize(this);
                UpdateWebResources.Initialize(this);
                OpenOptions.Initialize(this);
                CreateWebResource.Initialize(this);
                GenerateCode.Initialize(this);
            }
        }
        #endregion
    }
}
