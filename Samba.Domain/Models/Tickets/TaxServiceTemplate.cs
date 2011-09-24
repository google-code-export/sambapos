using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public enum ServiceAmountType
    {
        Percent,
        Amount
    }

    public class TaxServiceTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AmountType { get; set; }
        public decimal Amount { get; set; }
    }
}
