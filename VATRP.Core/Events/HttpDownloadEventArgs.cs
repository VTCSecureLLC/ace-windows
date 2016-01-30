using System;

namespace VATRP.Core.Events
{
    public class HttpDownloadEventArgs : EventArgs
    {
        public bool Succeeded;
        public bool Timeout;
        public bool Cancelled;
        public string URI;
        public HttpDownloadEventArgs(bool timeout, bool cancelled)
        {
            Succeeded = (!cancelled && !timeout);
            this.Timeout = timeout;
            this.Cancelled = cancelled;
        }
    }
}
