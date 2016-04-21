using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class NetworkUtility
    {
        public static string INTERNET_UNAVAILABLE = "Please check your network connection and try again.";

        public static bool IsInternetAvailable()
        {
            try
            {
                // Liz E. - ToDo later - we may prefer to add a method that allows us to test if the cdn provider is available, and another to test if the 
                //   domain for the provider is available. Note: Google will work ipv4 & ipv6
                Dns.GetHostEntry("www.google.com"); //using System.Net;
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

        public static bool IsCDNAvailable()
        {
            try
            {
                // Liz E. - ToDo later - we may prefer to add a method that allows us to test if the cdn provider is available, and another to test if the 
                //   domain for the provider is available. Note: Google will work ipv4 & ipv6
                Dns.GetHostEntry(ServiceManager.CDN_DOMAIN); //using System.Net;
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
