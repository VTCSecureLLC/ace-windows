namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneFirewallPolicy
    {
        LinphonePolicyNoFirewall, // Do not use any mechanism to pass through firewalls */
        LinphonePolicyUseNatAddress, // Use the specified public address */
        LinphonePolicyUseStun, // Use a STUN server to get the public address */
        LinphonePolicyUseIce, // Use the ICE protocol */
        LinphonePolicyUseUpnp, // Use the uPnP protocol */
    }
}