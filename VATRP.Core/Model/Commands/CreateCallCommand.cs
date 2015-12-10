﻿using System;

namespace VATRP.Core.Model.Commands
{
    internal class CreateCallCommand : LinphoneCommand
    {
        private readonly IntPtr _callParamsPtr;
        private readonly bool _enableRtt;
        private readonly string _callee;
        private readonly bool _muteMicrophone;

        public CreateCallCommand(IntPtr callParamsPtr, string callee, bool isRttOn, bool muteMicrophone)
            : base(LinphoneCommandType.CreateCall)
        {
            _callParamsPtr = callParamsPtr;
            _callee = callee;
            _enableRtt = isRttOn;
            _muteMicrophone = muteMicrophone;
        }

        public IntPtr CallParamsPtr
        {
            get { return _callParamsPtr; }
        }

        public bool EnableRtt
        {
            get { return _enableRtt; }
        }

        public bool MuteMicrophone
        {
            get { return _muteMicrophone; }
        }

        public string Callee
        {
            get { return _callee; }
        }
    }
}