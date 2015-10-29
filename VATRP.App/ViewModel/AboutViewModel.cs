using System;
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private string _appVersion;
        private string _appName;
        private string _copyright;

        public AboutViewModel()
        {
            LoadVersion();
        }

        private void LoadVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            var version = assembly.GetName().Version;

            AppName = assembly.GetName().Name;
            AppVersion = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
            Copyright = "Copyright 2015";
        }

        #region Properties

        public string AppName
        {
            get { return _appName; }
            set
            {
                _appName = value; 
                OnPropertyChanged("AppName");
            }
        }

        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = value; 
                OnPropertyChanged("AppVersion");
            }
        }

        public string Copyright
        {
            get { return _copyright; }
            set
            {
                _copyright = value; 
                OnPropertyChanged("Copyright");
            }
        }

        #endregion

    }
}