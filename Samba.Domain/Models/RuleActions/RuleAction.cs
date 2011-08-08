using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.RuleActions
{
    public class RuleAction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ActionType { get; set; }
        public string Parameter { get; set; }
    }
}
