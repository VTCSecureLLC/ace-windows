using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for Dialpad.xaml
    /// </summary>
    public partial class Dialpad
    {
        public event EventHandler<KeyPadEventArgs> KeypadClicked; 
        public Dialpad() : base(VATRPWindowType.DIALPAD_VIEW)
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ctrlDialpad.KeypadPressed += OnKeypadPressed;
        }

        private void OnKeypadPressed(object sender, KeyPadEventArgs e)
        {
            if (KeypadClicked != null)
            {
                KeypadClicked(this, new KeyPadEventArgs(e.Key));
            }
        }
    }
}
