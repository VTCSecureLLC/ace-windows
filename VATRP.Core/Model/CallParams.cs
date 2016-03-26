using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VATRP.Core.Model
{
    public class CallParams
    {
        public string DisplayName;
        public string Username;
        public string HostAddress;
        public int HostPort;

        public CallParams()
        {
            DisplayName = String.Empty;
            Username = String.Empty;
            HostAddress = String.Empty;
        }
    }
}
