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

        public UnifiedSettingsAudioVideoCtrl()
        {
            InitializeComponent();
            Title = "Audio/Video";
            this.Loaded += UnifiedSettingsAudioVideoCtrl_Loaded;
        }

        // ToDo VATRP987 - Liz E. these need to be hooked into acutal settings. not sure where they live.
        private void UnifiedSettingsAudioVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mute Speaker Call Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
           // ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
           //     Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mute Microphone Call Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
//            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
  //              Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnEchoCancel(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Echo Cancel Call Clicked");
            bool enabled = this.EchoCancelCheckBox.IsChecked ?? false;
           // ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
             //   Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Self View Clicked");
            bool enabled = ShowSelfViewCheckBox.IsChecked ?? false;
            //ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
              //  Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }



    }
}
