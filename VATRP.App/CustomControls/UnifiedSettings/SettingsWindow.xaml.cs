using com.vtcsecure.ace.windows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private UnifiedSettings_AccountChange AccountChangeRequested;
        private CallViewCtrl _callControl;
        private BaseUnifiedSettingsPanel _currentContent;
        private BaseUnifiedSettingsPanel _previousContent;
        private List<BaseUnifiedSettingsPanel> _allPanels;

        public SettingsWindow(CallViewCtrl callControl, UnifiedSettings_AccountChange accountChangeRequestedMethod)
        {
            InitializeComponent();
            
            AccountChangeRequested += accountChangeRequestedMethod;

            _allPanels = new List<BaseUnifiedSettingsPanel>();

            BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
            BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
            BaseUnifiedSettingsPanel.EnableSuperSettings = false;

            AccountSettings.ShowSettingsUpdate += HandleShowSettingsUpdate;
            InitializePanelAndEvents(AccountSettings);

            InitializePanelAndEvents(GeneralSettings);

            InitializePanelAndEvents(AudioVideoSettings);

            InitializePanelAndEvents(ThemeSettings);

            InitializePanelAndEvents(TextSettings);

            SummarySettings.ShowSettingsUpdate += HandleShowSettingsUpdate;
            InitializePanelAndEvents(SummarySettings);

//            InitializePanelAndEvents(AudioSettings);

//            InitializePanelAndEvents(VideoSettings);

//            InitializePanelAndEvents(CallSettings);

//            InitializePanelAndEvents(NetworkSettings);

            InitializePanelAndEvents(AdvancedSettings);

//            InitializePanelAndEvents(_viewTechnicalSupportPanel);

            _currentContent = GeneralSettings;
#if DEBUG
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#else
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#endif
            SetCallControl(callControl);

            this.Loaded += SettingsWindow_Loaded;
        }

        void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void SetCallControl(CallViewCtrl callControl)
        {
            _callControl = callControl;
            TextSettings.CallControl = _callControl;
//            AudioSettings.CallControl = _callControl;
            AudioVideoSettings.CallControl = _callControl;
        }

        private void InitializePanelAndEvents(BaseUnifiedSettingsPanel panel)
        {
            if (panel == null)
                return;

            if (!_allPanels.Contains(panel))
            {
                _allPanels.Add(panel);
            }
//            panel.ContentChanging += HandleContentChanging;
            panel.AddAccountChangedMethod(HandleAccountChangeRequested);
//            panel.AccountChangeRequested += HandleAccountChangeRequested;
        }

        #region ShowSettingsLevel
        public void HandleShowSettingsUpdate(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: BaseUnifiedSettingsPanel.EnableAdvancedSettings = show;
                    if (show)
                    {
                        AdvancedTab.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case UnifiedSettings_LevelToShow.Debug: BaseUnifiedSettingsPanel.EnabledDebugSettings = show;
                    break;
                case UnifiedSettings_LevelToShow.Normal: BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
                    BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
                    BaseUnifiedSettingsPanel.EnableSuperSettings = false;
                    break;
                case UnifiedSettings_LevelToShow.Super: BaseUnifiedSettingsPanel.EnableSuperSettings = show;
                    break;
                default:
                    break;
            }
            foreach (BaseUnifiedSettingsPanel panel in _allPanels)
            {
                panel.ShowSettings(settingsType, show);
            }
        }
        #endregion

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Close Clicked");
            _currentContent.SaveData();
            this.Hide();
        }

        private void SetHidden()
        {
            _currentContent.SaveData();
            this.Hide();
        }

//        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
//        {
//            //base.OnClosing(e);
//            e.Cancel = true;
//            SetHidden();
//        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentContent.SaveData();
            if (GeneralTab.IsSelected)
            {
                _currentContent = GeneralSettings;
            }
            else if (AudioVideoTab.IsSelected)
            {
                _currentContent = AudioVideoSettings;
            }
            else if (TextTab.IsSelected)
            {
                _currentContent = TextSettings;
            }
            else if (ThemeTab.IsSelected)
            {
                _currentContent = ThemeSettings;
            }
            else if (SummaryTab.IsSelected)
            {
                _currentContent = SummarySettings;
            }
            else if (AccountTab.IsSelected)
            {
                _currentContent = AccountSettings;
            }
            else if (MediaTab.IsSelected)
            {
//                _currentContent = MediaSettings;
            }
            else if (TestingTab.IsSelected)
            {
//                _currentContent = TestingSettings;
            }
            else if (AdvancedTab.IsSelected)
            {
                _currentContent = AdvancedSettings;
            }
        }


        #region respondToMenuChange
        public void RespondToMenuUpdate(ACEMenuSettingsUpdateType menuSetting)
        {
            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: UpdateVideoSettingsIfOpen(menuSetting);
                    break;
                default:
                    break;
            }
        }

        private void UpdateVideoSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
            if (AdvancedSettings.IsLoaded)
            {
                AdvancedSettings.UpdateForMenuSettingChange(menuSetting);
            }
            if (AudioVideoSettings.IsLoaded)
            {
                AudioVideoSettings.UpdateForMenuSettingChange(menuSetting);
            }
        }
        private void UpdateAudioSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
//            if (AdvancedSettings.IsLoaded)
//            {
//                AdvancedSettings.UpdateForMenuSettingChange(menuSetting);
//            }
            if (AudioVideoSettings.IsLoaded)
            {
                AudioVideoSettings.UpdateForMenuSettingChange(menuSetting);
            }
        }
        #endregion

        #region respondToRegistrationChange
        private void HandleAccountChangeRequested(ACEMenuSettingsUpdateType changeType)
        {
            if (AccountChangeRequested != null)
            {
                AccountChangeRequested(changeType);
            }
            // ToDo - this handle updates in the UI of the settings, if needed
        }

        #endregion


    }
}
