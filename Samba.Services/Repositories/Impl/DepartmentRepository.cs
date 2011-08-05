using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<Department>))]
    public class DepartmentRepository : RepositoryBase<Department>, IRepository<Department>
    {
        [ImportingConstructor]
        public DepartmentRepository(IWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
