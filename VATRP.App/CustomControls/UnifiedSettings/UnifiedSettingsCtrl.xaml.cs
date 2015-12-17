using com.vtcsecure.ace.windows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{

    /// <summary>
    /// Interaction logic for UnifiedSettingsCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsCtrl : UserControl
    {
        public UnifiedSettings_AccountChange AccountChangeRequested;

        private CallViewCtrl _callControl;

        private UnifiedSettingsMainCtrl _mainPanel;
        private UnifiedSettingsGeneralCtrl _generalPanel;
        private UnifiedSettingsAudioVideoCtrl _audioVideoPanel;
        private UnifiedSettingsThemeCtrl _themePanel;
        private UnifiedSettingsTextCtrl _textPanel;
        private UnifiedSettingsSummaryCtrl _summaryPanel;

        private UnifiedSettingsAudioCtrl _audioSettingsPanel;
        private UnifiedSettingsVideoCtrl _videoSettingsPanel;
        private UnifiedSettingsCallCtrl _callSettingsPanel;
        private UnifiedSettingsNetworkCtrl _networkSettingsPanel;
        private UnifiedSettingsAdvancedCtrl _advancedSettingsPanel;

        private BaseUnifiedSettingsPanel _currentContent;
        private List<BaseUnifiedSettingsPanel> _previousContent;

        private List<BaseUnifiedSettingsPanel> _allPanels;

        public UnifiedSettingsCtrl()
        {
            InitializeComponent();
            _previousContent = new List<BaseUnifiedSettingsPanel>();
            _allPanels = new List<BaseUnifiedSettingsPanel>();

            BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
            BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
            BaseUnifiedSettingsPanel.EnableSuperSettings = false;

            _mainPanel = new UnifiedSettingsMainCtrl();
            _allPanels.Add(_mainPanel);
            _mainPanel.ContentChanging += HandleContentChanging;
            _mainPanel.AccountChangeRequested += HandleAccountChangeRequested;
            _mainPanel.ShowSettingsUpdate += HandleShowSettingsUpdate;

            _generalPanel = new UnifiedSettingsGeneralCtrl();
            _allPanels.Add(_generalPanel);
            _generalPanel.ContentChanging += HandleContentChanging;

            _audioVideoPanel = new UnifiedSettingsAudioVideoCtrl();
            _allPanels.Add(_audioVideoPanel);
            _audioVideoPanel.ContentChanging += HandleContentChanging;

            _themePanel = new UnifiedSettingsThemeCtrl();
            _allPanels.Add(_themePanel);
            _themePanel.ContentChanging += HandleContentChanging;

            _textPanel = new UnifiedSettingsTextCtrl();
            _allPanels.Add(_textPanel);
            _textPanel.ContentChanging += HandleContentChanging;

            _summaryPanel = new UnifiedSettingsSummaryCtrl();
            _allPanels.Add(_summaryPanel);
            _summaryPanel.ContentChanging += HandleContentChanging;
            _summaryPanel.ShowSettingsUpdate += HandleShowSettingsUpdate;

            _audioSettingsPanel = new UnifiedSettingsAudioCtrl();
            _allPanels.Add(_audioSettingsPanel);
            _audioSettingsPanel.ContentChanging += HandleContentChanging;

            _videoSettingsPanel = new UnifiedSettingsVideoCtrl();
            _allPanels.Add(_videoSettingsPanel);
            _videoSettingsPanel.ContentChanging += HandleContentChanging;

            _callSettingsPanel = new UnifiedSettingsCallCtrl();
            _allPanels.Add(_callSettingsPanel);
            _callSettingsPanel.ContentChanging += HandleContentChanging;

            _networkSettingsPanel = new UnifiedSettingsNetworkCtrl();
            _allPanels.Add(_networkSettingsPanel);
            _networkSettingsPanel.ContentChanging += HandleContentChanging;

            _advancedSettingsPanel = new UnifiedSettingsAdvancedCtrl();
            _allPanels.Add(_advancedSettingsPanel);
            _advancedSettingsPanel.ContentChanging += HandleContentChanging;

            _currentContent = _mainPanel;
#if DEBUG
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#else
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#endif
            UpdateContentInUI();
        }

        public void Initialize()
        {
            _mainPanel.Initialize();
            // the other panels are initialized when they are shown
        }

        public void SetCallControl(CallViewCtrl callControl)
        {
            _callControl = callControl;
            _audioSettingsPanel.CallControl = _callControl;
            _audioVideoPanel.CallControl = _callControl;
        }

        private void UpdateContentInUI()
        {
            // in case current content is not set, revert to main panel to 'restart'
            if (_currentContent == null)
            {
                Console.WriteLine("UnifiedSettings: Navigation error - _currentContent is null");
                _currentContent = _mainPanel;
                _previousContent.Clear();
                this.ContentPanel.Content = _mainPanel;
            }
            _currentContent.Initialize();
            this.ContentPanel.Content = _currentContent;
            this.TitleLabel.Content = _currentContent.Title;

            if (_currentContent == _mainPanel)
            {
                BackLabel.Content = "";
            }
            else
            {
                BackLabel.Content = "< Back";
            }
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Back Clicked");
            if (_currentContent == _mainPanel)
            {
                // ignore
                return;
            }
            if (_previousContent.Count > 0)
            {
                // beacuse this comes from the main control not from within the content, 
                //   make sure that the current data is saved before changing content
                _currentContent.SaveData();

                // set the new content in this panel
                _currentContent = _previousContent[_previousContent.Count - 1];
                // pop the panel that is now the current panel off the previous stack
                _previousContent.Remove(_currentContent);

                UpdateContentInUI();
            }
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("About Clicked");

            UpdateContentInUI();
        }

        #region ShowSettingsLevel
        public void HandleShowSettingsUpdate(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: BaseUnifiedSettingsPanel.EnableAdvancedSettings = show;
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

        #region panel navigation
        private void HandleContentChanging(UnifiedSettingsContentType newContentType)
        {
            switch (newContentType)
            {
                case UnifiedSettingsContentType.MainContent: // clear previous, set to main
                    _currentContent.SaveData();
                    _previousContent.Clear();
                    _currentContent = _mainPanel;
                    UpdateContentInUI();
                    break;
                case UnifiedSettingsContentType.GeneralContent: MoveToContentPanel(_generalPanel);
                    break;
                case UnifiedSettingsContentType.AudioVideoContent: MoveToContentPanel(_audioVideoPanel);
                    break;
                case UnifiedSettingsContentType.ThemeContent: MoveToContentPanel(_themePanel);
                    break;
                case UnifiedSettingsContentType.TextContent: MoveToContentPanel(_textPanel);
                    break;
                case UnifiedSettingsContentType.SummaryContent: MoveToContentPanel(_summaryPanel);
                    break;
                case UnifiedSettingsContentType.AudioSettingsContent: MoveToContentPanel(_audioSettingsPanel);
                    break;
                case UnifiedSettingsContentType.VideoSettingsContent: MoveToContentPanel(_videoSettingsPanel);
                    break;
                case UnifiedSettingsContentType.CallSettingsContent: MoveToContentPanel(_callSettingsPanel);
                    break;
                case UnifiedSettingsContentType.NetworkSettingsContent: MoveToContentPanel(_networkSettingsPanel);
                    break;
                case UnifiedSettingsContentType.AdvancedSettingsContent: MoveToContentPanel(_advancedSettingsPanel);
                    break;

                default: break;
            }
        }

        private void MoveToContentPanel(BaseUnifiedSettingsPanel newPanel)
        {
            _currentContent.SaveData();

            _previousContent.Add(_currentContent);
            _currentContent = newPanel;
            UpdateContentInUI();
        }
        #endregion

        #region respondToMenuChange
        public void RespondToMenuUpdate(ACEMenuSettingsUpdateType menuSetting)
        {
            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                default:
                    break;
            }
        }

        private void UpdateAudioSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
            if (_audioSettingsPanel.IsLoaded)
            {
                _audioSettingsPanel.UpdateForMenuSettingChange(menuSetting);
            }
            if (_audioVideoPanel.IsLoaded)
            {
                _audioVideoPanel.UpdateForMenuSettingChange(menuSetting);
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
