// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-06-08 00:32

using System;
using System.Windows.Forms;
using D365VsTools.Configuration;
using D365VsTools.Extensions;
using D365VsTools.Forms;
using D365VsTools.VisualStudio;
using Microsoft.Xrm.Sdk;

namespace D365VsTools.Common
{
    /// <summary>
    /// Provides methods for uploading and publishing web resources
    /// </summary>
    public class CommandExecutor 
    {
        private Settings settings;
        public Settings Settings => settings ?? (settings = GetSettings());

        protected Settings GetSettings()
        {
            var result = ProjectHelper.GetSettings<Settings>();

            if (result?.Connection != null)
                return result;

            if (ProjectHelper.ShowErrorDialog() == DialogResult.Yes)
                result = ShowConfigurationDialog(result);

            return result;
        }
        

        /// <summary>
        /// Shows Configuration Dialog
        /// </summary>
        /// <param name="settings_"></param>
        /// <returns>Returns result of a configuration dialog</returns>
        public Settings ShowConfigurationDialog(Settings settings_ = null)
        {
            var optionsForm = OptionsForm.Instance;

            if (settings_ == null)
                settings_ = ProjectHelper.GetSettings<Settings>();

            if (settings_ != null)
                optionsForm.Init(settings_);
            else
                settings_ = new Settings();

            optionsForm.ShowDialog();

            if (optionsForm.DialogResult == DialogResult.OK)
            {
                settings_.Connection = optionsForm.SelectedConnection;
                settings_.Solution = optionsForm.SelectedSolution;
                settings_.AutoPublish = optionsForm.AutoPublish;
                settings_.IgnoreExtensions = optionsForm.IgnoreExtensions;
                settings_.ExtendedLog = optionsForm.ExtendedLog;

                ProjectHelper.SaveSettings(settings_);
            }
            return settings_;
        }
        
        public void Execute(Action<IOrganizationService> serviceAction, Settings settings_ = null, string errorMessage = null)
        {
            try
            {   
                var optionsForm = OptionsForm.Instance;
                optionsForm.Init(settings_ ?? Settings);
                optionsForm.ConnectAndExecute(serviceAction);
            }
            catch (Exception e)
            {
                LogError(errorMessage ?? "Error executing service action", e);
            }
        }
        
        protected static void LogError(string message, Exception error = null)
        {
            var errorMessage = error?.GetErrorMessage();
            Logger.WriteLine(string.Join(Environment.NewLine, message, errorMessage));
        }
    }
}