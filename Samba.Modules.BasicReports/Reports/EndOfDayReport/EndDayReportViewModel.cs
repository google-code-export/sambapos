using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Modules.BasicReports.Reports.EndOfDayReport
{
    public class EndDayReportViewModel : ReportViewModelBase
    {
        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override FlowDocument GetReport()
        {
            var currentPeriod = ReportContext.CurrentWorkPeriod;

            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, currentPeriod, Resources.WorkPeriodReport);

            //---------------

            report.AddColumTextAlignment("Departman", TextAlignment.Left, TextAlignment.Right);
            report.AddTable("Departman", Resources.Sales, "");

            var ticketGropus = ReportContext.Tickets
                .GroupBy(x => new { x.DepartmentId })
                .Select(x => new DepartmentInfo
                {
                    DepartmentId = x.Key.DepartmentId,
                    TicketCount = x.Count(),
                    Amount = x.Sum(y => y.GetSumWithoutTax()),
                    Tax = x.Sum(y => y.CalculateTax(y.GetPlainSum(), y.GetTotalDiscounts()))
                });

            if (ticketGropus.Count() > 1)
            {
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Departman", departmentInfo.DepartmentName, departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            report.AddRow("Departman", Resources.TotalSales.ToUpper(), ticketGropus.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            var taxSum = ticketGropus.Sum(x => x.Tax);
            if (taxSum > 0)
            {
                report.AddRow("Departman", Resources.TaxTotal.ToUpper(), taxSum.ToString(ReportContext.CurrencyFormat));
                report.AddRow("Departman", Resources.GrandTotal.ToUpper(), ticketGropus.Sum(x => x.Amount + x.Tax).ToString(ReportContext.CurrencyFormat));
            }
            //---------------

            var ac = ReportContext.GetOperationalAmountCalculator();

            report.AddColumnLength("GelirlerTablosu", "45*", "Auto", "35*");
            report.AddColumTextAlignment("GelirlerTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("GelirlerTablosu", Resources.Incomes, "", "");
            report.AddRow("GelirlerTablosu", Resources.Cash, ac.CashPercent, ac.CashTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.CreditCard, ac.CreditCardPercent, ac.CreditCardTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.Voucher, ac.TicketPercent, ac.TicketTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.AccountBalance, ac.AccountPercent, ac.AccountTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("GelirlerTablosu", Resources.TotalIncome.ToUpper(), "", ac.TotalAmount.ToString(ReportContext.CurrencyFormat));

            //---------------

            //Kasa raporu eklendiği için kasa özeti bu rapordan kaldırıldı. Başka bir rapora taşınabilir şimdilik bıraktım.

            //var cashTransactionTotal = ReportContext.GetCashTotalAmount();
            //var creditCardTransactionTotal = ReportContext.GetCreditCardTotalAmount();
            //var ticketTransactionTotal = ReportContext.GetTicketTotalAmount();

            //report.AddColumnLength("Kasa", "25*", "18*", "18*", "18*", "21*");
            //report.AddColumTextAlignment("Kasa", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right);
            //report.AddTable("Kasa", "Kasa", "Nakit", "K.Kartı", "Y.Çeki", "Toplam");
            //report.AddRow("Kasa", "Gün Başı",
            //    currentPeriod.CashAmount.ToString(ReportContext.CurrencyFormat),
            //    currentPeriod.CreditCardAmount.ToString(ReportContext.CurrencyFormat),
            //    currentPeriod.TicketAmount.ToString(ReportContext.CurrencyFormat),
            //    (currentPeriod.CashAmount + currentPeriod.CreditCardAmount + currentPeriod.TicketAmount).ToString(ReportContext.CurrencyFormat));

            //report.AddRow("Kasa", "Faaliyet",
            //                ac.CashTotal.ToString(ReportContext.CurrencyFormat),
            //                ac.CreditCardTotal.ToString(ReportContext.CurrencyFormat),
            //                ac.TicketTotal.ToString(ReportContext.CurrencyFormat),
            //                ac.GrandTotal.ToString(ReportContext.CurrencyFormat));

            //report.AddRow("Kasa", "Hareketler",
            //                cashTransactionTotal.ToString(ReportContext.CurrencyFormat),
            //                creditCardTransactionTotal.ToString(ReportContext.CurrencyFormat),
            //                ticketTransactionTotal.ToString(ReportContext.CurrencyFormat),
            //                (cashTransactionTotal + creditCardTransactionTotal + ticketTransactionTotal).ToString(ReportContext.CurrencyFormat));

            //var totalCash = currentPeriod.CashAmount + ac.CashTotal + cashTransactionTotal;
            //var totalCreditCard = currentPeriod.CreditCardAmount + ac.CreditCardTotal + creditCardTransactionTotal;
            //var totalTicket = currentPeriod.TicketAmount + ac.TicketTotal + ticketTransactionTotal;

            //report.AddRow("Kasa", "TOPLAM",
            //    totalCash.ToString(ReportContext.CurrencyFormat),
            //    totalCreditCard.ToString(ReportContext.CurrencyFormat),
            //    totalTicket.ToString(ReportContext.CurrencyFormat),
            //    (totalCash + totalCreditCard + totalTicket).ToString(ReportContext.CurrencyFormat));


            //---------------

            var propertySum = ReportContext.Tickets
                .SelectMany(x => x.TicketItems)
                .Sum(x => x.GetPropertyPrice() * x.Quantity);

            var voids = ReportContext.Tickets
                .SelectMany(x => x.TicketItems)
                .Where(x => x.Voided)
                .Sum(x => x.GetItemValue());

            var discounts = ReportContext.Tickets
                .SelectMany(x => x.Discounts)
                .Sum(x => x.DiscountAmount);

            var gifts = ReportContext.Tickets
                .SelectMany(x => x.TicketItems)
                .Where(x => x.Gifted)
                .Sum(x => x.GetItemValue());

            report.AddColumTextAlignment("Bilgi", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Bilgi", "65*", "35*");
            report.AddTable("Bilgi", Resources.GeneralInformation, "");
            report.AddRow("Bilgi", Resources.ItemProperties, propertySum.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", Resources.VoidsTotal, voids.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", Resources.DiscountsTotal, discounts.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", Resources.GiftsTotal, gifts.ToString(ReportContext.CurrencyFormat));

            if (ticketGropus.Count() > 1)
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Bilgi", departmentInfo.DepartmentName, departmentInfo.TicketCount);
                }

            var ticketCount = ticketGropus.Sum(x => x.TicketCount);

            report.AddRow("Bilgi", Resources.TicketCount, ticketCount);

            report.AddRow("Bilgi", Resources.SalesDivTicket, ticketCount > 0
                ? (ticketGropus.Sum(x => x.Amount) / ticketGropus.Sum(x => x.TicketCount)).ToString(ReportContext.CurrencyFormat)
                : "0");

            if (ticketGropus.Count() > 1)
            {
                foreach (var departmentInfo in ticketGropus)
                {
                    var dPayments = ReportContext.Tickets
                        .Where(x => x.DepartmentId == departmentInfo.DepartmentId)
                        .SelectMany(x => x.Payments)
                        .GroupBy(x => new { x.PaymentType })
                        .Select(x => new TenderedAmount { PaymentType = x.Key.PaymentType, Amount = x.Sum(y => y.Amount) });

                    report.AddColumnLength(departmentInfo.DepartmentName + Resources.Incomes, "40*", "Auto", "35*");
                    report.AddColumTextAlignment(departmentInfo.DepartmentName + Resources.Incomes, TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddTable(departmentInfo.DepartmentName + Resources.Incomes, string.Format(Resources.Incomes_f, departmentInfo.DepartmentName), "", "");
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.Cash, GetPercent(0, dPayments), GetAmount(0, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.CreditCard, GetPercent(1, dPayments), GetAmount(1, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.Voucher, GetPercent(2, dPayments), GetAmount(2, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.TotalIncome, "", departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));

                    var dvoids = ReportContext.Tickets
                        .Where(x => x.DepartmentId == departmentInfo.DepartmentId)
                        .SelectMany(x => x.TicketItems)
                        .Where(x => x.Voided)
                        .Sum(x => x.GetItemValue());

                    var ddiscounts = ReportContext.Tickets
                        .Where(x => x.DepartmentId == departmentInfo.DepartmentId)
                        .SelectMany(x => x.Discounts)
                        .Sum(x => x.DiscountAmount);

                    var dgifts = ReportContext.Tickets
                        .Where(x => x.DepartmentId == departmentInfo.DepartmentId)
                        .SelectMany(x => x.TicketItems)
                        .Where(x => x.Gifted)
                        .Sum(x => x.GetItemValue());

                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.VoidsTotal, "", dvoids.ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.DiscountsTotal, "", ddiscounts.ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + Resources.Incomes, Resources.GiftsTotal, "", dgifts.ToString(ReportContext.CurrencyFormat));
                }
            }

            //--

            if (ReportContext.Tickets.Select(x => x.GetTagData()).Where(x => !string.IsNullOrEmpty(x)).Distinct().Count() > 0)
            {
                var dict = new Dictionary<string, List<Ticket>>();

                foreach (var ticket in ReportContext.Tickets.Where(x => !string.IsNullOrEmpty(x.Tag)))
                {
                    var tags = ticket.Tag.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                    {
                        if (!dict.ContainsKey(tag))
                            dict.Add(tag, new List<Ticket>());
                        dict[tag].Add(ticket);
                    }
                }

                var tagGroups = dict.Select(x => new TicketTagInfo { Amount = x.Value.Sum(y => y.GetSumWithoutTax()), TicketCount = x.Value.Count, TagName = x.Key }).OrderBy(x => x.TagName);

                var tagGrp = tagGroups.GroupBy(x => x.TagName.Split(':')[0]);

                report.AddColumTextAlignment("Etiket", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Etiket", "45*", "Auto", "35*");
                report.AddTable("Etiket", Resources.TicketTags, "", "");

                foreach (var grp in tagGrp)
                {
                    report.AddBoldRow("Etiket", grp.Key, "", "");
                    foreach (var ticketTagInfo in grp)
                    {
                        report.AddRow("Etiket",
                            ticketTagInfo.TagName.Split(':')[1],
                            ticketTagInfo.TicketCount,
                            ticketTagInfo.Amount.ToString(ReportContext.CurrencyFormat));
                    }
                    var tag = ReportContext.TicketTagGroups.SingleOrDefault(x => x.Name == grp.Key);
                    if (tag != null)
                    {
                        var totalAmount = grp.Sum(x => x.Amount);
                        report.AddRow("Etiket", string.Format(Resources.TotalAmount_f, tag.Name), "", totalAmount.ToString(ReportContext.CurrencyFormat));

                        var sum = 0m;

                        if (tag.NumericTags)
                        {
                            try
                            {
                                sum = grp.Sum(x => Convert.ToDecimal(x.TagName.Split(':')[1]) * x.TicketCount);
                                report.AddRow("Etiket", string.Format(Resources.TicketTotal_f, tag.Name), "", sum.ToString("#,##.##"));
                            }
                            catch (FormatException)
                            {
                                report.AddRow("Etiket", string.Format(Resources.TicketTotal_f, tag.Name), "", "#Hata!");
                            }
                        }
                        else
                        {
                            sum = grp.Sum(x => x.TicketCount);
                        }
                        if (sum > 0)
                        {
                            var average = totalAmount / sum;
                            report.AddRow("Etiket", string.Format(Resources.TotalAmountDivTag_f, tag.Name), "", average.ToString(ReportContext.CurrencyFormat));
                        }
                    }
                }
            }

            //----

            var owners = ReportContext.Tickets.SelectMany(ticket => ticket.TicketItems.Select(ticketItem => new { Ticket = ticket, TicketItem = ticketItem }))
                .GroupBy(x => new { x.TicketItem.CreatingUserId })
                .Select(x => new UserInfo { UserId = x.Key.CreatingUserId, Amount = x.Sum(y => MenuGroupBuilder.CalculateTicketItemTotal(y.Ticket, y.TicketItem)) });

            report.AddColumTextAlignment("Garson", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Garson", "65*", "35*");
            report.AddTable("Garson", Resources.UserSales, "");

            foreach (var ownerInfo in owners)
            {
                report.AddRow("Garson", ownerInfo.UserName, ownerInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems);

            report.AddColumTextAlignment("Gıda", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("Gıda", "45*", "Auto", "35*");
            report.AddTable("Gıda", Resources.ItemSales, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("Gıda", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("Gıda", Resources.Total.ToUpper(), "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            return report.Document;
        }

        private static string GetPercent(int paymentType, IEnumerable<TenderedAmount> data)
        {
            var total = data.Sum(x => x.Amount);
            return total > 0 ? string.Format("%{0:0.00}", (GetAmount(paymentType, data) * 100) / total) : "%0";
        }

        private static decimal GetAmount(int paymentType, IEnumerable<TenderedAmount> data)
        {
            var r = data.SingleOrDefault(x => x.PaymentType == paymentType);
            return r != null ? r.Amount : 0;
        }

        protected override string GetHeader()
        {
            return Resources.WorkPeriodReport;
        }
    }
}
