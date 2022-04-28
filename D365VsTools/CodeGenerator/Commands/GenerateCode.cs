using System;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using D365VsTools.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace D365VsTools.CodeGenerator.Commands
{
    internal sealed class GenerateCode
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0500;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;
        
        private XrmCodeGenerator generator;
        
        public static GenerateCode Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateCode"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GenerateCode(Package package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ProjectGuids.ItemCommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new GenerateCode(package);
        }
        
        private void MenuItemCallback(object sender, EventArgs e)
        {
            Logger.WriteLine("Executing Generate Code - Start");

            var filePath = ProjectHelper.GetSelectedFilePath();
            if (string.IsNullOrWhiteSpace(filePath))
                return;
            
            generator = generator ?? new XrmCodeGenerator();
            generator.GenerateCode(filePath);
            
            Logger.WriteLine("Executing Generate Code - End");
        }
    }
}
