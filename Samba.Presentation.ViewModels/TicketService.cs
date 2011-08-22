using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public class TicketService
    {
        public static void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            AppServices.MainDataContext.Recalculate(ticket);
            if (total != ticket.TotalAmount)
            {
                RuleExecutor.NotifyEvent(RuleEventNames.TicketTotalChanged,
                    new
                    {
                        Ticket = ticket,
                        TicketTotal = ticket.GetSum(),
                        DiscountTotal = ticket.GetTotalDiscounts(),
                        GiftTotal = ticket.GetTotalGiftAmount(),
                        PaymentTotal = ticket.GetPaymentAmount()
                    });
            }
        }
    }
}
