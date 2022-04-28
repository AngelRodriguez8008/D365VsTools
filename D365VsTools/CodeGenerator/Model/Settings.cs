using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
namespace D365VsTools.CodeGenerator.Model
{
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            EntityList = new ObservableCollection<string>();
            EntitiesSelected = new ObservableCollection<string>();

            Dirty = false;
        }

        #region boiler-plate INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            Dirty = true;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion


        private string _OutputPath;
        private string _Namespace;
        private string _Template;
        private string _T4Path;

        private string _ProjectName;
        public string ProjectName
        {
            get => _ProjectName;
            set => SetField(ref _ProjectName, value);
        }
        public string T4Path
        {
            get => _T4Path;
            set => SetField(ref _T4Path, value);
        }
        public string Template
        {
            get => _Template;
            set
            {
                SetField(ref _Template, value);
                NewTemplate = !System.IO.File.Exists(System.IO.Path.Combine(_Folder, _Template));
            }
        }
        private string _Folder = "";
        public string Folder
        {
            get => _Folder;
            set => SetField(ref _Folder, value);
        }

        private bool _NewTemplate;
        public bool NewTemplate
        {
            get => _NewTemplate;
            set => SetField(ref _NewTemplate, value);
        }
        public string OutputPath
        {
            get => _OutputPath;
            set => SetField(ref _OutputPath, value);
        }

        private ObservableCollection<string> _TemplateList = new ObservableCollection<string>();
        public ObservableCollection<string> TemplateList
        {
            get => _TemplateList;
            set => SetField(ref _TemplateList, value);
        }

        private ObservableCollection<string> _EntityList;
        public ObservableCollection<string> EntityList
        {
            get => _EntityList;
            set => SetField(ref _EntityList, value);
        }

        private ObservableCollection<string> _EntitiesSelected;
        public ObservableCollection<string> EntitiesSelected
        {
            get => _EntitiesSelected;
            set => SetField(ref _EntitiesSelected, value);
        }
        public bool IsReadOnly => _MappingSettings != null;

        private MappingSettings _MappingSettings;
        public MappingSettings MappingSettings
        {
            get => _MappingSettings;
            set => SetField(ref _MappingSettings, value);
        }


        public string Namespace
        {
            get => _Namespace;
            set => SetField(ref _Namespace, value);
        }

        public bool Dirty { get; set; }

    }

    public class MappingSettings
    {
        public Dictionary<string, EntityMappingSetting> Entities;
    }
    public class EntityMappingSetting
    {
        public string CodeName;
        public Dictionary<string, string> Attributes;
    }
}
