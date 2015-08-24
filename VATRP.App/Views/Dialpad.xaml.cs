using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VATRP.App.Interfaces;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for Dialpad.xaml
    /// </summary>
    public partial class Dialpad : IMediaBox
    {
        private bool _isActivated = false;
        public Dialpad()
        {
            InitializeComponent();
        }

        public bool IsActivated
        {
            get { return _isActivated; }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            _isActivated = true;
        }

        private void Dialpad_OnClosing(object sender, CancelEventArgs e)
        {
            if (!App.AllowDestroyWindows)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}
