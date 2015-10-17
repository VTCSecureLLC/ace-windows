using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VATRP.App.Model;

namespace VATRP.App.Views
{
    public partial class KeyPadCtrl
    {
        public event EventHandler<KeyPadEventArgs> KeypadClicked; 
        public KeyPadCtrl(): base(VATRPWindowType.KEYPAD_VIEW)
        {
            InitializeComponent();
        }

        private void buttonKeyPad_Click(object sender, RoutedEventArgs e)
        {
            if (KeypadClicked != null)
            {
                var btnKey = e.OriginalSource as Button;
                if (btnKey != null)
                {
                    if (Equals(e.OriginalSource, buttonKeyPadStar))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyStar));
                    }
                    else if (Equals(e.OriginalSource, buttonKeyPadPound))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyPound));
                    }
                    else
                    {
                        char key;
                        if ( char.TryParse(btnKey.Tag.ToString(), out key))
                            KeypadClicked(this, new KeyPadEventArgs((DialpadKey)key));
                        else
                        {
                            Debug.WriteLine("Failed to get keypad: " + btnKey.Tag);
                            KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_Key0));
                        }
                    }
                }
            }
        }
    }
}
