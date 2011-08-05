using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<PrinterMap>))]
    public class PrinterMapRepository : RepositoryBase<PrinterMap>, IRepository<PrinterMap>
    {
        [ImportingConstructor]
        public PrinterMapRepository(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
