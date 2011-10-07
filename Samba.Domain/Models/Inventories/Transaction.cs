using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class Transaction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public virtual IList<TransactionItem> TransactionItems { get; set; }

        public Transaction()
        {
            TransactionItems = new List<TransactionItem>();
            Date = DateTime.Now;
        }
    }
}
