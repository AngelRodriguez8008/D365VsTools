using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using D365VsTools.CodeGenerator.Model;
using Microsoft.VisualStudio.TextTemplating;

namespace D365VsTools.CodeGenerator.T4
{
    public class Generator
    {
        public static string ProcessTemplateBasic(string templateFileName, Context context)
        {
            AppDomain appDomain = null;

            try
            {
                appDomain = AppDomain.CreateDomain("T4AppDomain Labashosky");
                Compiler host = new Compiler(appDomain);
                Engine engine = new Engine();
                
                host.TemplateFileValue = templateFileName;

                //NAMESPACE is now hosted in context CallContext.LogicalSetData("Namespace", "CCGCore");
                CallContext.LogicalSetData("Context", context);
                
                string input = File.ReadAllText(templateFileName);

                string output = null;
                //output = engine.ProcessTemplate(input, host);

                if (host.Errors.HasErrors) { return host.Errors[0].ErrorText; }

                return output;
            }
            finally
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }
    }
}