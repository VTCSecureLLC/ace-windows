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
using VATRP.App.Interfaces;
using VATRP.App.Model;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for ContactBox.xaml
    /// </summary>
    public partial class ContactBox
    {
        private bool _isActivated = false;
        public ContactBox() : base(VATRPWindowType.CONTACT_VIEW)
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
