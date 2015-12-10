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
    /// Interaction logic for UnifiedSettingsThemeCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsThemeCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsThemeCtrl()
        {
            InitializeComponent();
            Title = "Theme";
        }
        // ToDo VATRP-988 - implement color picker, connect Force  508
        //   Sample color picker: http://www.codeproject.com/Articles/33001/WPF-A-Simple-Color-Picker-With-Preview
        private void OnForegroundColor(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Foreground Color Clicked");
        }
        private void OnBackgroundColor(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Background Color Clicked");
        }
        private void OnForce508(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Force 508 Clicked");
        }

    }
}
