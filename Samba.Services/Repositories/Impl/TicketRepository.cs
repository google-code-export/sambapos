using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Services.Repositories.Impl
{
    [Export(typeof(IRepository<Ticket>))]
    public class TicketRepository : RepositoryBase<Ticket>, IRepository<Ticket>
    {
        [ImportingConstructor]
        public TicketRepository(IWorkspace ws)
            : base(ws)
        {
        }
    }
}
