using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;

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
            AddDefaultReportHeader(report, currentPeriod, "Gün Sonu Raporu");

            //---------------

            report.AddColumTextAlignment("Departman", TextAlignment.Left, TextAlignment.Right);
            report.AddTable("Departman", "Satışlar", "");

            var ticketGropus = ReportContext.Tickets
                .GroupBy(x => new { x.DepartmentId })
                .Select(x => new DepartmentInfo { DepartmentId = x.Key.DepartmentId, TicketCount = x.Count(), Amount = x.Sum(y => y.GetSum()) });

            if (ticketGropus.Count() > 1)
            {
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Departman", departmentInfo.DepartmentName, departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));
                }
            }

            report.AddRow("Departman", "TOPLAM Satışlar", ticketGropus.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));

            //---------------

            var ac = ReportContext.GetOperationalAmountCalculator();

            report.AddColumnLength("Gelirler", "45*", "Auto", "35*");
            report.AddColumTextAlignment("Gelirler", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddTable("Gelirler", "Gelirler", "", "");
            report.AddRow("Gelirler", "Nakit", ac.CashPercent, ac.CashTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Gelirler", "Kredi Kartı", ac.CreditCardPercent, ac.CreditCardTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Gelirler", "Yemek Çeki", ac.TicketPercent, ac.TicketTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Gelirler", "Açık Hesap", ac.AccountPercent, ac.AccountTotal.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Gelirler", "TOPLAM Gelir", "", ac.TotalAmount.ToString(ReportContext.CurrencyFormat));

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
            report.AddTable("Bilgi", "Bilgiler", "");
            report.AddRow("Bilgi", "İlave Özellikler", propertySum.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", "İptaller Toplamı", voids.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", "İskontolar Toplamı", discounts.ToString(ReportContext.CurrencyFormat));
            report.AddRow("Bilgi", "İkramlar Toplamı", gifts.ToString(ReportContext.CurrencyFormat));

            if (ticketGropus.Count() > 1)
                foreach (var departmentInfo in ticketGropus)
                {
                    report.AddRow("Bilgi", departmentInfo.DepartmentName, departmentInfo.TicketCount);
                }

            var ticketCount = ticketGropus.Sum(x => x.TicketCount);

            report.AddRow("Bilgi", "Adisyon Sayısı", ticketCount);

            report.AddRow("Bilgi", "Ciro / Adisyon", ticketCount > 0
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

                    report.AddColumnLength(departmentInfo.DepartmentName + "Gelirler", "40*", "Auto", "35*");
                    report.AddColumTextAlignment(departmentInfo.DepartmentName + "Gelirler", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                    report.AddTable(departmentInfo.DepartmentName + "Gelirler", departmentInfo.DepartmentName + " Gelirleri", "", "");
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "Nakit", GetPercent(0, dPayments), GetAmount(0, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "Kredi Kartı", GetPercent(1, dPayments), GetAmount(1, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "Yemek Çeki", GetPercent(2, dPayments), GetAmount(2, dPayments).ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "TOPLAM Gelir", "", departmentInfo.Amount.ToString(ReportContext.CurrencyFormat));

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

                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "İptaller Toplamı", "", dvoids.ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "İskontolar Toplamı", "", ddiscounts.ToString(ReportContext.CurrencyFormat));
                    report.AddRow(departmentInfo.DepartmentName + "Gelirler", "İkramlar Toplamı", "", dgifts.ToString(ReportContext.CurrencyFormat));
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

                var tagGroups = dict.Select(x => new TicketTagInfo { Amount = x.Value.Sum(y => y.GetSum()), TicketCount = x.Value.Count, TagName = x.Key }).OrderBy(x => x.TagName);

                var tagGrp = tagGroups.GroupBy(x => x.TagName.Split(':')[0]);

                report.AddColumTextAlignment("Etiket", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("Etiket", "45*", "Auto", "35*");
                report.AddTable("Etiket", "Adisyon Etiketleri", "", "");

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
                        report.AddRow("Etiket", string.Format("{0} Toplam Tutarı:", tag.Name), "", totalAmount.ToString(ReportContext.CurrencyFormat));

                        var sum = 0m;

                        if (tag.NumericTags)
                        {
                            try
                            {
                                sum = grp.Sum(x => Convert.ToDecimal(x.TagName.Split(':')[1]) * x.TicketCount);
                                report.AddRow("Etiket", string.Format("Toplam {0}:", tag.Name), "", sum.ToString("#,##.##"));
                            }
                            catch (FormatException)
                            {
                                report.AddRow("Etiket", string.Format("Toplam {0}:", tag.Name), "", "#Hata!");
                            }
                        }
                        else
                        {
                            sum = grp.Sum(x => x.TicketCount);
                        }

                        var average = totalAmount / sum;
                        report.AddRow("Etiket", string.Format("Toplam Tutar / {0}:", tag.Name), "", average.ToString(ReportContext.CurrencyFormat));
                    }
                }
            }

            //----

            var owners = ReportContext.Tickets.SelectMany(ticket => ticket.TicketItems.Select(ticketItem => new { Ticket = ticket, TicketItem = ticketItem }))
                .GroupBy(x => new { x.TicketItem.CreatingUserId })
                .Select(x => new UserInfo { UserId = x.Key.CreatingUserId, Amount = x.Sum(y => MenuGroupBuilder.CalculateTicketItemTotal(y.Ticket, y.TicketItem)) });

            report.AddColumTextAlignment("Garson", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Garson", "65*", "35*");
            report.AddTable("Garson", "Kullanıcı Bazlı Satışlar", "");

            foreach (var ownerInfo in owners)
            {
                report.AddRow("Garson", ownerInfo.UserName, ownerInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems);

            report.AddColumTextAlignment("Gıda", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("Gıda", "45*", "Auto", "35*");
            report.AddTable("Gıda", "Ürün Satışları", "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("Gıda", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("Gıda", "Toplam", "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
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
            return "Gün Sonu Raporu";
        }
    }
}
