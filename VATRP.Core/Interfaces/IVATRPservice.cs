using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VATRP.Core.Interfaces
{
    public interface IVATRPservice
    {
        bool Start();
        bool Stop();

        event EventHandler<EventArgs> ServiceStarted;
        event EventHandler<EventArgs> ServiceStopped;
    }
}
