namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneRegistrationState
    {
        LinphoneRegistrationNone, // Initial state for registrations
        LinphoneRegistrationProgress, // Registration is in progress
        LinphoneRegistrationOk, // Registration is successful
        LinphoneRegistrationCleared, // Unregistration succeeded
        LinphoneRegistrationFailed // Registration failed
    };
}