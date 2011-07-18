using System.ComponentModel.Composition;
using Samba.Domain.Foundation;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(ICurrencyRepository))]
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly IWorkspace _workspace;

        [ImportingConstructor]
        public CurrencyRepository(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void UpdateCurrencyContext(CurrencyContext context)
        {
            _workspace.Update(context);
        }

        public CurrencyContext GetCurrencyContext()
        {
            return _workspace.Single<CurrencyContext>(x => x.Currency == "TL");
        }
    }
}
