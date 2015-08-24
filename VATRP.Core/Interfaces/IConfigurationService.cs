using System;
using VATRP.Core.Events;
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IConfigurationService
    {
        bool Start();

        bool Stop();

        string Get(Configuration.ConfSection section, Configuration.ConfEntry entry, string defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, string value);

        int Get(Configuration.ConfSection section, Configuration.ConfEntry entry, int defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, int value);

        bool Get(Configuration.ConfSection section, Configuration.ConfEntry entry, bool defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, bool value);

        void SaveConfig();

        event EventHandler<ConfigurationEventArgs> onConfigurationEvent;

        void Reset();
    }
}
