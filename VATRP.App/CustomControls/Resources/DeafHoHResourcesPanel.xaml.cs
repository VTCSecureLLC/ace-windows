using com.vtcsecure.ace.windows.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    /// <summary>
    /// Interaction logic for DeafHoHResourcesPanel.xaml
    /// </summary>
    public partial class DeafHoHResourcesPanel : BaseResourcePanel
    {
        private const string CDN_RESOURCE_URI = "http://cdn.vatrp.net/numbers.json";
        private List<ResourceInfo> resourceInfoList;

        public DeafHoHResourcesPanel()
        {
            InitializeComponent();
            List<ResourceInfo> resourceInfoList = new List<ResourceInfo>();

            Title = "Deaf/Hard of Hearing Resources";
            this.Loaded += DeafHoHResourcesPanel_Loaded;
        }

        private void DeafHoHResourcesPanel_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateListOfResources();
        }

        // load resources from http://cdn.vatrp.net/numbers.json
        private void PopulateListOfResources()
        {
            List<ResourceInfo> resourceList = LoadListOfResources();
            foreach (ResourceInfo item in resourceList)
            {
                ResourceInfoListView.Items.Add(item);
            }

        }

        private List<ResourceInfo> LoadListOfResources()
        {
            WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CDN_RESOURCE_URI);
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = request.GetResponse();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                List<ResourceInfo> resourceList;
                try
                {
                    // deserialize json to ResourceInfo List
                    resourceList = JsonDeserializer.JsonDeserialize<List<ResourceInfo>>(jsonResults.ToString());
                    return resourceList;
                }
                catch (Exception ex)
                {
                    string message = "Failed to parse resource information. Details: " + ex.Message;
                    return new List<ResourceInfo>();
                }
            }
            catch (Exception ex)
            {
                string message = "Failed to get resource information. Details: " + ex.Message;
                return new List<ResourceInfo>();
            }
            finally 
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void ResourceInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ResourceInfoListView.SelectedItem;
            if (selectedItem != null)
            {
                ResourceInfo resourceInfo = (ResourceInfo)selectedItem;
                Console.WriteLine("Resource Selected: Name=" + resourceInfo.name + " Address=" + resourceInfo.address);
                OnCallResourceRequested(resourceInfo);
            }
            else
            {
                Console.WriteLine("No selected resource available");
            }
        }

    }
}
