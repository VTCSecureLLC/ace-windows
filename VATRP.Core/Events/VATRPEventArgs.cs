using System;
using System.Collections.Generic;

namespace VATRP.Core.Events
{
    public class VATRPEventArgs : EventArgs
    {
        private readonly IDictionary<String, Object> extras;

        public VATRPEventArgs()
            :base()
        {
            this.extras = new Dictionary<String, Object>();
        }

        public Object GetExtra(String key)
        {
            if (this.extras.ContainsKey(key))
            {
                return this.extras[key];
            }
            return null;
        }

        public VATRPEventArgs AddExtra(String key, Object value)
        {
            if (!this.extras.ContainsKey(key))
            {
                this.extras.Add(key, value);
            }
            return this;
        }
    }
}
