using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<ScreenMenu>))]
    public class ScreenMenuRepository : RepositoryBase<ScreenMenu>, IRepository<ScreenMenu>
    {
        [ImportingConstructor]
        public ScreenMenuRepository(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
