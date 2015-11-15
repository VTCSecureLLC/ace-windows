
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IProviderService : IVATRPservice
    {
        void Save();
        bool AddProvider(VATRPServiceProvider provider);

        bool DeleteProvider(VATRPServiceProvider provider);

        bool ContainsProvider(VATRPServiceProvider provider);

        int GetProvidersCount();
        void ClearProvidersList();

        string[] GetProviderList();
        VATRPServiceProvider FindProvider(string providerID);
    }
}
