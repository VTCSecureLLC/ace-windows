using System;

namespace VATRP.Core.Model.Commands
{
    internal class ToggleCameraCommand : CallCommand
    {
        public readonly bool EnableCamera;
        public ToggleCameraCommand(IntPtr callPtr, bool mute)
            : base(LinphoneCommandType.ToggleCamera, callPtr)
        {
            EnableCamera = mute;
        }
    }
}