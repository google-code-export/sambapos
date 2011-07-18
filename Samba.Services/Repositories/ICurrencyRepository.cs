using Samba.Domain.Foundation;

namespace Samba.Services.Repositories
{
    public interface ICurrencyRepository
    {
        CurrencyContext GetCurrencyContext();
        void UpdateCurrencyContext(CurrencyContext context);
    }
}
