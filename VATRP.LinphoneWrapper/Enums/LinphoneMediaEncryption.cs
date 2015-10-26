using System.ComponentModel;

namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneMediaEncryption
    {
        [Description("None")]
        LinphoneMediaEncryptionNone, /**< No media encryption is used */
        [Description("SRTP")]
        LinphoneMediaEncryptionSRTP, /**< Use SRTP media encryption */
        [Description("ZRTP")]
        LinphoneMediaEncryptionZRTP, /**< Use ZRTP media encryption */
        [Description("DTLS")]
        LinphoneMediaEncryptionDTLS /**< Use DTLS media encryption */
    }
}