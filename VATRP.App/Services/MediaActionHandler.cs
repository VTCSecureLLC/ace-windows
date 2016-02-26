using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Services
{
    internal static class MediaActionHandler
    {
        internal static bool MakeVideoCall(string remoteUri)
        {
            ILinphoneService _linphoneService = ServiceManager.Instance.LinphoneService;

            if (!_linphoneService.CanMakeVideoCall())
            {
                MessageBox.Show("Video call not supported yet.", "ACE", MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return false;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "ACE", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            // VATRP-2506, 2496: check to see if there is currently a call in progress. IF there is a call in progress, prevent 
            //   an outgoing call. 
            int callCount = ServiceManager.Instance.LinphoneService.GetActiveCallsCount;
            if (callCount > 0)
            {
                return false;  // i think this can be a quiet failure - the user is already in a call.
            }
            
            bool muteMicrophone = false;
            bool muteSpeaker = false;
//            bool enableVideo = true;
            if (App.CurrentAccount != null)
            {
                muteMicrophone = App.CurrentAccount.MuteMicrophone;
                muteSpeaker = App.CurrentAccount.MuteSpeaker;
//                enableVideo = App.CurrentAccount.EnableVideo;
            }

            var target = string.Empty;

            string un, host;
            int port;
            VATRPCall.ParseSipAddress(remoteUri, out un, out host, out port);

            Regex rE164 = new Regex(@"^(\+|00)?[1-9]\d{1,14}$");
            bool isE164 = rE164.IsMatch(un);

            if (!host.NotBlank())
            {
                // set proxy to selected provider
                // find selected provider host
                var provider =
                    ServiceManager.Instance.ProviderService.FindProviderLooseSearch(
                        ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                            Configuration.ConfEntry.CURRENT_PROVIDER, ""));

                if (provider != null)
                {
                    target = string.Format("sip:{0}@{1}", un, provider.Address);
                }
                else if (App.CurrentAccount != null) 
                    target = string.Format("sip:{0}@{1}", un, App.CurrentAccount.ProxyHostname);
            }
            else
            {
                target = string.Format("sip:{0}@{1}:{2}", un, host, port);
            }

            if (isE164)
                target += ";user=phone";

            // update video policy settings prior to making a call
            _linphoneService.MakeCall(target, true, ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, true, ServiceManager.Instance.LocationString);
            return true;
        }
       
    }
}
