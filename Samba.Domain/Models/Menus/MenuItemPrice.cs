using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemPrice
    {
        public int Id { get; set; }
        public int MenuItemPortionId { get; set; }
        public string PriceTag { get; set; }
        public decimal Price { get; set; }
    }
}
