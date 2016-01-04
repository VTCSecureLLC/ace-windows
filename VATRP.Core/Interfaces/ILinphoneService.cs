﻿using System;
using System.Collections.Generic;
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
        event LinphoneService.OnMessageReceivedDelegate OnChatMessageReceivedEvent;
        event LinphoneService.OnMessageStatusChangedDelegate OnChatMessageStatusChangedEvent;
        event LinphoneService.OnCallLogUpdatedDelegate OnLinphoneCallLogUpdatedEvent;
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
        void ClearProxyInformation();
        void ClearAccountInformation();
        void MakeCall(string destination, bool videoOn, bool rttEnabled, bool muteMicrophone, bool muteSpeaker, string geolocation);
        void AcceptCall(IntPtr callPtr, bool rttEnabled, bool muteMicrophone, bool muteSpaker);
        void DeclineCall(IntPtr callPtr);
        bool TerminateCall(IntPtr callPtr);
        void ResumeCall(IntPtr callPtr);
        void PauseCall(IntPtr callPtr);
        void AcceptRTTProposition(IntPtr callPtr);
        void SendRTTProposition(IntPtr callPtr);

        bool SendChar(uint charCode, IntPtr callPtr, ref IntPtr chatRoomPtr, ref IntPtr chatMsgPtr);
        void MuteCall(bool isMuted);
        bool IsCallMuted();
        void ToggleMute();
        void MuteSpeaker(bool isMuted);
        bool IsSpeakerMuted();
        void ToggleVideo(bool enableVideo, IntPtr callPtr);
        void SendDtmf(VATRPCall call, char dtmf);
        bool IsCameraEnabled(IntPtr callPtr);
        void EnableVideo(bool enable, bool automaticallyInitiate, bool automaticallyAccept);
        bool IsEchoCancellationEnabled();
        void EnableEchoCancellation(bool enable);
        bool IsSelfViewEnabled();
        void EnableSelfView(bool enable);
        void SwitchSelfVideo();
        void SetVideoPreviewWindowHandle(IntPtr hWnd, bool reset = false);
        void SetPreviewVideoSize(MSVideoSize w, MSVideoSize h);
        void SetVideoCallWindowHandle(IntPtr hWnd, bool reset = false);
        bool IsVideoEnabled(VATRPCall call);
        void UpdateMediaSettings(VATRPAccount account);
        bool UpdateNativeCodecs(VATRPAccount account, CodecType codecType);
        void configureFmtpCodec();
        void FillCodecsList(VATRPAccount account, CodecType codecType);
        bool UpdateNetworkingParameters(VATRPAccount account);
        void SetAVPFMode(LinphoneAVPFMode mode, LinphoneRTCPMode rtcpMode);
        int GetAVPFMode();
        IntPtr GetCallParams(IntPtr callPtr);
        string GetUsedAudioCodec(IntPtr callParams);
        string GetUsedVideoCodec(IntPtr callParams);
        MSVideoSizeDef GetVideoSize(IntPtr curparams, bool sending);
        float GetFrameRate(IntPtr curparams, bool sending);
        LinphoneMediaEncryption GetMediaEncryption(IntPtr curparams);
        LinphoneCallStats GetCallAudioStats(IntPtr callPtr);
        LinphoneCallStats GetCallVideoStats(IntPtr callPtr);
        void GetUsedPorts(out int sipPort, out int rtpPort);
        LinphoneChatMessageState GetMessageStatus(IntPtr intPtr);
        bool SendChatMessage(VATRPChat chat, string message, ref IntPtr msgPtr);
        void MarkChatAsRead(IntPtr chatRoomPtr);
        int GetHistorySize(string username);
        void LoadChatRoom(VATRPChat chat);
        void EnableAdaptiveRateControl(bool bEnable);
        IntPtr LinphoneCore { get; }
        int GetActiveCallsCount { get; }

        List<VATRPDevice> GetAvailableCameras();
        void SetCamera(string deviceId);
        VATRPDevice GetSelectedCamera();

        List<VATRPDevice> GetAvailableMicrophones();
        void SetCaptureDevice(string deviceId);
        VATRPDevice GetSelectedMicrophone();

        List<VATRPDevice> GetAvailableSpeakers();
        void SetSpeakers(string deviceId);
        VATRPDevice GetSelectedSpeakers();

    }
}