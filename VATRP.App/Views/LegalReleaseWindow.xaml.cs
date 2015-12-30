using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Services;
using VATRP.LinphoneWrapper;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for LegalReleaseWindow.xaml
    /// </summary>
    public partial class LegalReleaseWindow : Window
    {
        public LegalReleaseWindow()
        {
            InitializeComponent();
        }

        private void LoadCompleted(object sender, RoutedEventArgs e)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (!string.IsNullOrEmpty(path))
            {
                var legalReleasePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "legal_release.txt");

                try
                {
                    if (File.Exists(legalReleasePath))
                    {
                        var textRange = new TextRange(RtfContainer.Document.ContentStart,
                            RtfContainer.Document.ContentEnd);
                        using (var fileStream = new FileStream(legalReleasePath, FileMode.Open, FileAccess.Read))
                        {
                            textRange.Load(fileStream, DataFormats.Text);
                        }
                    }
                    else
                        throw new FileNotFoundException("legal_release.txt");
                }
                catch (Exception ex)
                {
                    ServiceManager.LogError("Load Legal release", ex);
                }
            }
        }

        private void AcceptAgreement(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void DeclineAgreement(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var scroll = (ScrollViewer)((Border)VisualTreeHelper.GetChild(RtfContainer, 0)).Child;
            if (scroll != null)
            {
                scroll.ScrollChanged += OnScrollChanged;
            }
            else
            {
                BtnAccept.IsEnabled = true;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scroll = (sender as ScrollViewer);
            if (scroll != null) 
                BtnAccept.IsEnabled = (scroll.VerticalOffset == scroll.ScrollableHeight);
        }
    }
}
