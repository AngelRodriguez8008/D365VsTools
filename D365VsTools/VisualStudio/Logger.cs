using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace D365VsTools.VisualStudio
{
    public static class Logger
    {
        private static IVsOutputWindow _outputWindow;

        /// <summary>
        /// Initialize Logger output window
        /// </summary>
        public static void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid windowGuid = ProjectGuids.OutputWindow;
            string windowTitle = "D365-VS-Tools";
            _outputWindow?.CreatePane(ref windowGuid, windowTitle, 1, 1);
        }

        /// <summary>
        /// Adds line feed to message and writes it to output window
        /// </summary>
        /// <param name="message">text message to write</param>
        /// <param name="print">print or ignore call using for extended logging</param>
        public static void WriteLine(string message, bool print = true)
        {
            if (print)
                Write(message + "\r\n");
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public static void Write(string message)
        {
            try
            {
                Debug.Print(message);
                ThreadHelper.ThrowIfNotOnUIThread();

                Guid windowGuid = ProjectGuids.OutputWindow;
                IVsOutputWindowPane pane;
                _outputWindow.GetPane(ref windowGuid, out pane);
                pane.Activate();
                pane.OutputString(message);
            }
            catch (Exception e)
            {
                Debug.Print("Error writting Log: " + e.Message);
            }
        }

        public static void Clear()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var windowGuid = ProjectGuids.OutputWindow;
            IVsOutputWindowPane pane;
            _outputWindow.GetPane(ref windowGuid, out pane);
            pane.Clear();
        }

        public static void WriteLineWithTime(string message, bool print = true)
        {
            WriteLine(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }
    }
}
