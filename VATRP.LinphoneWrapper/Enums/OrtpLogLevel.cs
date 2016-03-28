namespace VATRP.LinphoneWrapper.Enums
{
    public enum OrtpLogLevel
    {
        ORTP_DEBUG = 1,
        ORTP_TRACE = 1 << 1,
        ORTP_MESSAGE = 1 << 2,
        ORTP_WARNING = 1 << 3,
        ORTP_ERROR = 1 << 4,
        ORTP_FATAL = 1 << 5,
        ORTP_LOGLEV_END = 1 << 6
    }
}