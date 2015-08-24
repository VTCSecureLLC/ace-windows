using System;
using System.Collections.Generic;

namespace VATRP.Core.Events
{
    public class MyEventArgs : EventArgs
    {
        private readonly IDictionary<String, Object> extras;

        public MyEventArgs()
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

        public MyEventArgs AddExtra(String key, Object value)
        {
            if (!this.extras.ContainsKey(key))
            {
                this.extras.Add(key, value);
            }
            return this;
        }
    }
}
