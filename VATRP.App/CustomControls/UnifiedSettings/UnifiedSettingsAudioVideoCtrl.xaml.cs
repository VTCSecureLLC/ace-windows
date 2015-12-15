using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
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
    /// Interaction logic for UnifiedSettingsAudioVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioVideoCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;

        public UnifiedSettingsAudioVideoCtrl()
        {
            InitializeComponent();
            Title = "Audio/Video";
            this.Loaded += UnifiedSettingsAudioVideoCtrl_Loaded;
        }

        // ToDo VATRP987 - Liz E. these need to be hooked into acutal settings. not sure where they live.
        private void UnifiedSettingsAudioVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            if (App.CurrentAccount == null)
                return;

            MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
            MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
            MuteSpeakerCheckBox.IsEnabled = false;
            EchoCancelCheckBox.IsChecked = App.CurrentAccount.EchoCancel;
            ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
        }

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    break;
                default:
                    break;
            }
        }



        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Microphone Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }
        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Speaker Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteSpeaker)
            {
                App.CurrentAccount.MuteSpeaker = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }
        private void OnEchoCancel(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Echo Cancel Call Clicked");
            bool enabled = this.EchoCancelCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EchoCancel)
            {
                App.CurrentAccount.EchoCancel = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Show Self View Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = this.ShowSelfViewCheckBox.IsChecked ?? true;
            if (enable != App.CurrentAccount.ShowSelfView)
            {
                App.CurrentAccount.ShowSelfView = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }



    }
}
