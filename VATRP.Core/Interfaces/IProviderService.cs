
using System.Collections.Generic;
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
        List<VATRPServiceProvider> GetProviderListFullInfo();

        VATRPServiceProvider FindProvider(string providerID);
        VATRPServiceProvider FindProviderLooseSearch(string providerID);  // because the json domain does nto match our stored domains
    }
}
