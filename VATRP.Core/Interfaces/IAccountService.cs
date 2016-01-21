
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IAccountService : IVATRPservice
    {
        void Save();
        bool AddAccount(VATRPAccount account);
        bool DeleteAccount(VATRPAccount account);
        bool ContainsAccount(VATRPAccount account);
        int GetAccountsCount();
        void ClearAccounts();
        VATRPAccount FindAccount(string accountUID);
        VATRPAccount FindAccount(string username, string hostname);
    }
}
