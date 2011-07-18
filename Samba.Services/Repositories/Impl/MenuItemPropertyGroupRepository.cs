using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<MenuItemPropertyGroup>))]
    public class MenuItemPropertyGroupRepository : RepositoryBase<MenuItemPropertyGroup>, IRepository<MenuItemPropertyGroup>
    {
        [ImportingConstructor]
        public MenuItemPropertyGroupRepository(IWorkspace workspace)
            : base(workspace)
        { }
    }
}
