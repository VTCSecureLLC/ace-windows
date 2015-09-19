namespace VATRP.Core.Interfaces
{
    public interface IContactService : IVATRPservice
    {

        string GetContactDisplayName(Model.VATRPContact contact, string useraname);

        Model.VATRPContact FindContactByUsername(string useraname);
        Model.VATRPContact FindContactId(string id);
    }
}
