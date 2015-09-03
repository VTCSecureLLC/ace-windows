using System;

namespace VATRP.App.Model
{
    public class KeyPadEventArgs : EventArgs
    {
        private DialpadKey key;

        public KeyPadEventArgs(DialpadKey key)
        {
            this.key = key;
        }


        public DialpadKey Key
        {
            get { return key; }
        }
    }
}
