using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<PrinterTemplate>))]
    public class PrinterTemplateRepository : RepositoryBase<PrinterTemplate>, IRepository<PrinterTemplate>
    {
        [ImportingConstructor]
        public PrinterTemplateRepository(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
