
namespace VATRP.Core.Interfaces
{
    public interface ISoundService : IVATRPservice
    {
        void PlayRingTone();
        void StopRingTone();

        void PlayRingBackTone();

        void StopRingBackTone();
        void PlayNewEvent();

        void StopNewEvent();

        void PlayConnectionChanged(bool connected);
        void StopConnectionChanged(bool connected);
    }
}
