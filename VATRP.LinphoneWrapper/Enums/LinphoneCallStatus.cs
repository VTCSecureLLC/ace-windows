namespace VATRP.LinphoneWrapper.Enums
{

    // Enum representing the status of a call

    public enum LinphoneCallStatus
    {
        LinphoneCallSuccess, // The call was successful 
        LinphoneCallAborted, // The call was aborted 
        LinphoneCallMissed, // The call was missed (unanswered) 
        LinphoneCallDeclined // The call was declined, either locally or by remote end 
    }

}