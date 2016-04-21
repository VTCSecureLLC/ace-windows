using System;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Model
{
    public class DeclineMessageArgs : EventArgs
    {
        public string DeclineMessage { get; private set; }

        public VATRPContact Sender { get; set; }

        public DeclineMessageArgs(string msg)
        {
            this.DeclineMessage = msg;
        }

    }
}
