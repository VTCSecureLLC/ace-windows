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

        private CallViewCtrl _callControl;

        private UnifiedSettingsMainCtrl _mainPanel;
        private UnifiedSettingsGeneralCtrl _generalPanel;
        private UnifiedSettingsAudioVideoCtrl _audioVideoPanel;
        private UnifiedSettingsThemeCtrl _themePanel;
        private UnifiedSettingsTextCtrl _textPanel;
        private UnifiedSettingsSummaryCtrl _summaryPanel;

        private UnifiedSettingsAudioCtrl _audioSettingsPanel;
        private UnifiedSettingsVideoCtrl _videoSettingsPanel;

        private BaseUnifiedSettingsPanel _currentContent;
        private List<BaseUnifiedSettingsPanel> _previousContent;

        public UnifiedSettingsCtrl()
        {
            InitializeComponent();

            _mainPanel = new UnifiedSettingsMainCtrl();
            _mainPanel.ContentChanging += HandleContentChanging;

            _generalPanel = new UnifiedSettingsGeneralCtrl();
            _generalPanel.ContentChanging += HandleContentChanging;

            _audioVideoPanel = new UnifiedSettingsAudioVideoCtrl();
            _audioVideoPanel.ContentChanging += HandleContentChanging;

            _themePanel = new UnifiedSettingsThemeCtrl();
            _themePanel.ContentChanging += HandleContentChanging;

            _textPanel = new UnifiedSettingsTextCtrl();
            _textPanel.ContentChanging += HandleContentChanging;

            _summaryPanel = new UnifiedSettingsSummaryCtrl();
            _summaryPanel.ContentChanging += HandleContentChanging;

            _audioSettingsPanel = new UnifiedSettingsAudioCtrl();
            _audioSettingsPanel.ContentChanging += HandleContentChanging;

            _videoSettingsPanel = new UnifiedSettingsVideoCtrl();
            _videoSettingsPanel.ContentChanging += HandleContentChanging;

            _previousContent = new List<BaseUnifiedSettingsPanel>();
            _currentContent = _mainPanel;

            UpdateContentInUI();
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
//                case UnifiedSettingsContentType.TransportContent: MoveToContentPanel(_transportPanel);
//                    break;
//                case UnifiedSettingsContentType.TextSettingsContent: MoveToContentPanel(_textSettingsPanel);
//                    break;
                case UnifiedSettingsContentType.AudioSettingsContent: MoveToContentPanel(_audioSettingsPanel);
                    break;
                case UnifiedSettingsContentType.VideoSettingsContent: MoveToContentPanel(_videoSettingsPanel);
                    break;
//                case UnifiedSettingsContentType.CallSettingsContent: MoveToContentPanel(_callSettingsPanel);
//                    break;
//                case UnifiedSettingsContentType.NetworkSettingsContent: MoveToContentPanel(_networkSettingsPanel);
//                    break;
//                case UnifiedSettingsContentType.AdvancedSettingsContent: MoveToContentPanel(_advancedSettingsPanel);
//                    break;

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
        public void RespondToMenuUpdate(ACEMenuSettings menuSetting)
        {
            switch (menuSetting)
            {
                case ACEMenuSettings.MuteMicrophoneMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                default:
                    break;
            }
        }

        private void UpdateAudioSettingsIfOpen(ACEMenuSettings menuSetting)
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

    }
}
