namespace VATRP.App.Interfaces
{
    public interface ISettings
    {
        bool IsChanged();
        bool Save();
    }
}