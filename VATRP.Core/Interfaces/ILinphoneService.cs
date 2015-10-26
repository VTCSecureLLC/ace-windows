﻿using System;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Interfaces
{
    public interface ILinphoneService : IVATRPservice
    {
        event LinphoneService.GlobalStateChangedDelegate GlobalStateChangedEvent;
        event LinphoneService.RegistrationStateChangedDelegate RegistrationStateChangedEvent;
        event LinphoneService.CallStateChangedDelegate CallStateChangedEvent;
        event LinphoneService.ErrorDelegate ErrorEvent;
        event LinphoneService.NotifyReceivedDelegate NotifyReceivedEvent;
        event LinphoneService.CallStatisticsChangedDelegate CallStatisticsChangedEvent;
        event LinphoneService.IsComposingReceivedDelegate IsComposingReceivedEvent;
        LinphoneService.Preferences LinphoneConfig { get; }
        bool IsStarting { get; }
        bool IsStarted { get; }
        bool IsStopping { get; }
        bool IsStopped { get; }
        bool Start(bool enableLogs);
        void LockCalls();
        void UnlockCalls();
        bool CanMakeVideoCall();
        void SendDtmfAsSipInfo(bool use_info);
        void PlayDtmf(char dtmf, int duration);
        bool Register();
        bool Unregister(bool deferred);
        void MakeCall(string destination, bool videoOn);
        void AcceptCall(IntPtr callPtr);
        void DeclineCall(IntPtr callPtr);
        bool TerminateCall(IntPtr callPtr);
        void AcceptRTTProposition(IntPtr callPtr);
        void SendRTTProposition(IntPtr callPtr);

        bool SendChar(uint charCode, IntPtr callPtr, ref IntPtr chatRoomPtr, ref IntPtr chatMsgPtr);
        bool IsCallMuted();
        void ToggleMute();
        void SendDtmf(VATRPCall call, char dtmf);
        void EnableVideo(bool enable);
        void SwitchSelfVideo();
        void SetVideoPreviewWindowHandle(IntPtr hWnd, bool reset = false);
        void SetPreviewVideoSize(MSVideoSize w, MSVideoSize h);
        void SetVideoCallWindowHandle(IntPtr hWnd, bool reset = false);
        bool IsVideoEnabled(VATRPCall call);
        void UpdateMediaSettings(VATRPAccount account);
        bool UpdateNativeCodecs(VATRPAccount account, CodecType codecType);
        void FillCodecsList(VATRPAccount account, CodecType codecType);
        bool UpdateNetworkingParameters(VATRPAccount account);
        void SetAVPFMode(LinphoneAVPFMode mode);
        IntPtr GetCallParams(IntPtr callPtr);
        string GetUsedAudioCodec(IntPtr callParams);
        string GetUsedVideoCodec(IntPtr callParams);
        MSVideoSizeDef GetVideoSize(IntPtr curparams, bool sending);
        float GetFrameRate(IntPtr curparams, bool sending);
        LinphoneMediaEncryption GetMediaEncryption(IntPtr curparams);
        LinphoneCallStats GetCallAudioStats(IntPtr callPtr);
        LinphoneCallStats GetCallVideoStats(IntPtr callPtr);
        void GetUsedPorts(out int sipPort, out int rtpPort);
    }
}