using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using D365VsTools.CodeGenerator.Model;
using D365VsTools.CodeGenerator.T4;
using D365VsTools.Common;
using D365VsTools.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk;
using Context = D365VsTools.CodeGenerator.Model.Context;

namespace D365VsTools.CodeGenerator
{
    // http://blogs.msdn.com/b/vsx/archive/2013/11/27/building-a-vsix-deployable-single-file-generator.aspx
    [Guid(ProjectGuids.CodeGenerator)]
    public class XrmCodeGenerator
    {
        public const string Name = nameof(XrmCodeGenerator);
        public const string Description = "Microsoft Dataverse Code Generator for Visual Studio";

        private readonly CommandExecutor executor = new CommandExecutor();
        private string extension = "cs";
        

        public void GenerateCode(string inputFileName)
        {
            Logger.WriteLine("Loading Template File... ");
            string inputFileContent = File.ReadAllText(inputFileName);

            executor.Execute(service =>
            {
                Logger.WriteLine("Loading Entities & Attributes Mapping File... ");
                using (var mapper = CreateMapper(inputFileName))
                {
                    Logger.WriteLine("Creating Mapping Context... ");
                    Context context = CreateContext(service, mapper, out var error);

                    string generateCode;
                    if (error == null)
                    {
                        Logger.WriteLine("Generating code from template... ");
                        generateCode = BuildCode(context, inputFileName, inputFileContent);
                    }
                    else
                        generateCode = error;

                    if (generateCode == null)
                        return;
                    
                    var outputFileName = Path.ChangeExtension(inputFileName, extension);

                    File.WriteAllText(outputFileName, generateCode, Encoding.UTF8);
                }
            });
        }

        private Context CreateContext(IOrganizationService service, Mapper mapper, out string resultCode)
        {
            Context context = null;
            resultCode = null;
            try
            {
                context = mapper.CreateContext(service);
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                Logger.WriteLine(error);
                Logger.WriteLine(ex.StackTrace);
                Logger.WriteLine("Unable to map entities, see error above.");
                resultCode = error + Environment.NewLine + ex.StackTrace;
            }
            return context;
        }

        public string BuildCode(Context context, string inputFileName, string inputFileContent)
        {
            try
            {
                if (inputFileContent == null)
                    throw new ArgumentNullException(nameof(inputFileContent));

                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var t4 = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
                var sessionHost = t4 as ITextTemplatingSessionHost;
                if (sessionHost == null)
                {
                    var error = "Unexpected Error occur by Initializing the SessionHost. Abort";
                    Logger.WriteLine(error);
                    return error;
                }

                sessionHost.Session = sessionHost.CreateSession();
                sessionHost.Session["Context"] = context;

                var cb = new Callback();
                t4.BeginErrorSession();
                string content = t4.ProcessTemplate(inputFileName, inputFileContent, cb);
                t4.EndErrorSession();

                // If there was an output directive in the TemplateFile, then cb.SetFileExtension() will have been called.
                if (!string.IsNullOrWhiteSpace(cb.FileExtension))
                    extension = cb.FileExtension;

                if (cb.ErrorMessages.Count > 0)
                {
                    var errors = new StringBuilder();
                    //Configuration.Instance.DTE.ExecuteCommand("View.ErrorList");
                    // Append any error/warning to output window
                    foreach (var err in cb.ErrorMessages)
                    {
                        // The templating system (eg t4.ProcessTemplate) will automatically add error/warning to the ErrorList 
                        var errorLine = "[" + (err.Warning ? "WARN" : "ERROR") + "] " + err.Message + " " + err.Line + "," + err.Column;
                        errors.AppendLine(errorLine);
                        Logger.WriteLine(errorLine);
                    }
                    var error = errors.ToString();
                    Logger.WriteLine(error);
                    return error;
                }

                Logger.WriteLine("Writing code to disk... ");
                Logger.WriteLine("Done!");
                return content;
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                Logger.WriteLine(error);
                Logger.WriteLine(ex.StackTrace);
                Logger.WriteLine("Unable to map entities, see error above.");
                return error + Environment.NewLine + ex.StackTrace;
            }
        }

        private Mapper CreateMapper(string inputFileName)
        {
            var mappingFile = Path.ChangeExtension(inputFileName, "mapping.json");
            Logger.WriteLine($"Mapping File: {mappingFile ?? "null"}");
            if (string.IsNullOrWhiteSpace(mappingFile))
            {
                Logger.WriteLine("Mapping File not found. Abort");
                return null;
            }
            var mappingSettings = LoadMappingFromFile(mappingFile);

            var entities = mappingSettings?.Entities?.Keys;
            if (entities == null)
            {
                Logger.WriteLine("Mapping File not found. Abort"); // Todo: consider to generate all entities (I don't thing so)
                return null;
            }

            var logicalNames = string.Join("\n\t", entities);
            Logger.WriteLine($"Entities Mapping:\n\t{logicalNames}");

            return new Mapper(mappingSettings);
        }

        public static MappingSettings LoadMappingFromFile(string mappingFile)
        {
            Logger.WriteLine("Loading Mapping from File ...");
            bool exist = File.Exists(mappingFile);
            if (exist == false)
                return null;

            var json = File.ReadAllText(mappingFile);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var mapping = Deserialize<MappingSettings>(json);
            return mapping;
        }

        public static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return default;

            var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), settings);
                return serializer.ReadObject(memoryStream) as T;
            }
        }
    }
}
