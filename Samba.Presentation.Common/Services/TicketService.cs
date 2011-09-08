using Samba.Domain.Models.Tickets;
using Samba.Services;

namespace Samba.Presentation.Common.Services
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
                        PreviousTotal = total,
                        TicketTotal = ticket.GetSum(),
                        DiscountTotal = ticket.GetTotalDiscounts(),
                        DiscountAmount = ticket.GetDiscountAmount(),
                        TipAmount = ticket.GetTipAmount(),
                        GiftTotal = ticket.GetTotalGiftAmount(),
                        PaymentTotal = ticket.GetPaymentAmount()
                    });
            }
        }
    }
}
