using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
            this.Loaded += UnifiedSettingsThemeCtrl_Loaded;
        }

        void UnifiedSettingsThemeCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        // ToDo VATRP-988 - implement color picker, connect Force  508
        private void OnForegroundColor(object sender, RoutedEventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int colorInt = dlg.Color.ToArgb();

            }
        }
        private void OnBackgroundColor(object sender, RoutedEventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int colorInt = dlg.Color.ToArgb();

            }
        }

    }
}
