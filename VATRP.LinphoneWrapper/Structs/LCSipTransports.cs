namespace VATRP.LinphoneWrapper.Structs
{
    // Linphone core SIP transport ports.
    public struct LCSipTransports
    {
        public int udp_port; // udp port to listening on, negative value if not set
        public int tcp_port; // tcp port to listening on, negative value if not set
        public int dtls_port; // dtls port to listening on, negative value if not set
        public int tls_port; // tls port to listening on, negative value if not set
    };
}