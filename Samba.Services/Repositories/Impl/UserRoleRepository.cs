using System.ComponentModel.Composition;
using Samba.Domain.Models.Users;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<UserRole>))]
    public class UserRoleRepository : RepositoryBase<UserRole>, IRepository<UserRole>
    {
        
        [ImportingConstructor]
        public UserRoleRepository(IWorkspace workspace)
            : base(workspace)
        {
        
        }

        
    }
}
