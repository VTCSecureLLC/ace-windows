using com.vtcsecure.ace.windows.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Services;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    /// <summary>
    /// Interaction logic for ResourceMainCtrl.xaml
    /// </summary>
    public partial class ResourceMainCtrl : BaseResourcePanel
    {
        public ResourceMainCtrl()
        {
            InitializeComponent();
            this.Loaded += ControlLoaded;
            Title = "Resources";
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            // populate the version labels:
            //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var version = assembly.GetName().Version;

            //AceVersionLabel.Content = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);

            //string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            //LinphoneVersionLabel.Content = string.Format("Core Version {0}", linphoneLibraryVersion);
            AceLabel.Visibility = System.Windows.Visibility.Collapsed;
            AceVersionLabel.Visibility = System.Windows.Visibility.Collapsed;
            LinphoneLabel.Visibility = System.Windows.Visibility.Collapsed;
            LinphoneVersionLabel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnTechnicalSupport(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Technical Support Clicked");
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }

        private void OnInstantFeedback(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Instant Feedback Clicked");
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }
        private void OnDeafHoh(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Deaf/Hard of Hearing Resources Clicked");
            OnContentChanging(ResourcesType.DeafHoHResourcesContent);
        }

        private void OnSyncCardDAV(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Sync Contacts Clicked");
            ServiceManager.Instance.LinphoneService.CardDAVSync();
        }
    }
}
