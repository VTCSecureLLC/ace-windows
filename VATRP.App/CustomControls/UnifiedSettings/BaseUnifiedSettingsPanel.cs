using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    public enum UnifiedSettings_LevelToShow
    {
        Normal, 
        Advanced,
        Debug,
        Super
    }

    public delegate void UnifiedSettings_EnableSettings(UnifiedSettings_LevelToShow settingsType, bool show);
    public delegate void UnifiedSettings_ContentChanging(UnifiedSettingsContentType contentType);
    public delegate void UnifiedSettings_AccountChange(ACEMenuSettingsUpdateType changeType);


    public class BaseUnifiedSettingsPanel : UserControl
    {
        public static bool EnableAdvancedSettings;
        public static bool EnabledDebugSettings;
        public static bool EnableSuperSettings;

        // VATRP-1170: Show items that have been implemented but are not specified for windows.
        public static System.Windows.Visibility VisibilityForSuperSettingsAsPreview = System.Windows.Visibility.Visible;

        private MainControllerViewModel _parentViewModel;

        protected bool _initialized;
        public string Title { get; set; }
        // Call When Panel Content needs to change
        public event UnifiedSettings_ContentChanging ContentChanging;
        public event UnifiedSettings_AccountChange AccountChangeRequested;


        public void ShowSettings(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: ShowAdvancedOptions(show);
                    break;
                case UnifiedSettings_LevelToShow.Debug: ShowDebugOptions(show);
                    break;
                case UnifiedSettings_LevelToShow.Super: ShowSuperOptions(show);
                    break;
                default:   // show normal
                    ShowNormalOptions();
                    break;
            }
        }

        public void ShowNormalOptions()
        {
            ShowAdvancedOptions(false);
            ShowDebugOptions(false);
            ShowSuperOptions(false);
        }

        public virtual void ShowDebugOptions(bool show)
        {
        }

        public virtual void ShowAdvancedOptions(bool show)
        {
        }

        public virtual void ShowSuperOptions(bool show)
        {
            ShowDebugOptions(show);
            ShowAdvancedOptions(show);
        }

        public virtual void AddAccountChangedMethod(UnifiedSettings_AccountChange accountChangedHandler)
        {
            this.AccountChangeRequested += accountChangedHandler;
        }
        // Invoke the Content Changed event
        public virtual void OnContentChanging(UnifiedSettingsContentType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

        // Invoke the Account Change Requested event
        public virtual void OnAccountChangeRequested(ACEMenuSettingsUpdateType changeType)
        {
            if (AccountChangeRequested != null)
            {
                AccountChangeRequested(changeType);
            }
        }

        public virtual void Initialize()
        {
            _initialized = true;
            //ShowNormalOptions();
        }
		
        public virtual void SaveData()
        {
        }

        public virtual void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
        }

    }
}
