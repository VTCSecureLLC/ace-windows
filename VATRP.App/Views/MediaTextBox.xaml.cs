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
using System.Windows.Shapes;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for MediaTextWindow.xaml
    /// </summary>
    public partial class MediaTextWindow : Window
    {
        private bool _isActivated = false;
        public MediaTextWindow()
        {
            InitializeComponent();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.AllowDestroyWindows)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        public bool IsActivated
        {
            get { return _isActivated; }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            _isActivated = true;
        }
    }
}
