using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain;
using Samba.Domain.Models.Settings;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.CashReport
{
    public class CashReportViewModel : ReportViewModelBase
    {
        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        private static string GetPaymentString(int paymentType)
        {
            if (paymentType == (int)PaymentType.Cash) return "Nakit";
            if (paymentType == (int)PaymentType.CreditCard) return "K.Kartı";
            return "Y.Çeki";
        }

        private static string Fs(decimal amount)
        {
            return amount.ToString(ReportContext.CurrencyFormat);
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");
            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, "Kasa Raporu");

            if (ReportContext.CurrentWorkPeriod.Id == 0)
            {
                report.AddHeader(" ");
                report.AddHeader("Tarih aralığı aktif çalışma dönemi değil.");
                report.AddHeader("Değerler kasa devirlerini içermemektedir.");
            }

            var cashExpenseTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Cash && x.TransactionType == (int)TransactionType.Expense)
                .Sum(x => x.Amount);
            var creditCardExpenseTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.CreditCard && x.TransactionType == (int)TransactionType.Expense)
                .Sum(x => x.Amount);
            var ticketExpenseTotal = ReportContext.CashTransactions
               .Where(x => x.PaymentType == (int)PaymentType.Ticket && x.TransactionType == (int)TransactionType.Expense)
               .Sum(x => x.Amount);

            var cashIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Cash && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);
            var ticketIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.Ticket && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);
            var creditCardIncomeTotal = ReportContext.CashTransactions
                .Where(x => x.PaymentType == (int)PaymentType.CreditCard && x.TransactionType == (int)TransactionType.Income)
                .Sum(x => x.Amount);


            report.AddColumTextAlignment("Gider", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Gider", "15*", "Auto", "25*");
            report.AddTable("Gider", "Giderler", "", "");

            var expenseTransactions =
                ReportContext.CashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense);

            if (expenseTransactions.Count() > 0)
            {
                report.AddRow("Gider", "KASA HAREKETLERİ", "", "");
                foreach (var cashTransaction in expenseTransactions)
                {
                    report.AddRow("Gider", GetPaymentString(cashTransaction.PaymentType),
                        Fct(cashTransaction), Fs(cashTransaction.Amount));
                }
            }

            report.AddRow("Gider", "TOPLAMLAR", "", "");
            report.AddRow("Gider", GetPaymentString(0), "Toplam Gider", Fs(cashExpenseTotal));
            report.AddRow("Gider", GetPaymentString(1), "Toplam Gider", Fs(creditCardExpenseTotal));
            report.AddRow("Gider", GetPaymentString(2), "Toplam Gider", Fs(ticketExpenseTotal));
            report.AddRow("Gider", "GENEL TOPLAM", "", Fs(cashExpenseTotal + creditCardExpenseTotal + ticketExpenseTotal));

            var ac = ReportContext.GetOperationalAmountCalculator();

            report.AddColumTextAlignment("Gelir", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Gelir", "15*", "Auto", "25*");
            report.AddTable("Gelir", "Gelirler", "", "");

            if (ReportContext.CurrentWorkPeriod.Id > 0) //devreden rakamları aktif çalışma dönemlerinden biri seçildiyse çalışır
            {
                report.AddRow("Gelir", "DEVREDEN", "", "");

                if (ReportContext.CurrentWorkPeriod.CashAmount > 0)
                    report.AddRow("Gelir", GetPaymentString(0) + " Devreden", "", Fs(ReportContext.CurrentWorkPeriod.CashAmount));
                if (ReportContext.CurrentWorkPeriod.CreditCardAmount > 0)
                    report.AddRow("Gelir", GetPaymentString(1) + " Devreden", "", Fs(ReportContext.CurrentWorkPeriod.CreditCardAmount));
                if (ReportContext.CurrentWorkPeriod.TicketAmount > 0)
                    report.AddRow("Gelir", GetPaymentString(2) + " Devreden", "", Fs(ReportContext.CurrentWorkPeriod.TicketAmount));

                report.AddRow("Gelir", "Toplam Devreden", "", Fs(ReportContext.CurrentWorkPeriod.CashAmount
                                                                    + ReportContext.CurrentWorkPeriod.CreditCardAmount
                                                                    + ReportContext.CurrentWorkPeriod.TicketAmount));
            }


            var incomeTransactions =
                ReportContext.CashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income);

            if (incomeTransactions.Count() > 0)
            {
                report.AddRow("Gelir", "FAALİYET GELİRLERİ", "", "");
                if (ac.CashTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(0) + " Satış Gelirleri", "", Fs(ac.CashTotal));
                if (ac.CreditCardTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(1) + " Satış Gelirleri", "", Fs(ac.CreditCardTotal));
                if (ac.TicketTotal > 0)
                    report.AddRow("Gelir", GetPaymentString(2) + " Satış Gelirleri", "", Fs(ac.TicketTotal));

                report.AddRow("Gelir", "Toplam Faaliyet Geliri", "", Fs(ac.CashTotal
                                                               + ac.CreditCardTotal
                                                               + ac.TicketTotal));


                report.AddRow("Gelir", "KASA HAREKETLERİ", "", "");
                var it = 0m;
                foreach (var cashTransaction in incomeTransactions)
                {
                    it += cashTransaction.Amount;
                    report.AddRow("Gelir", GetPaymentString(cashTransaction.PaymentType),
                        Fct(cashTransaction),
                        Fs(cashTransaction.Amount));
                }

                report.AddRow("Gelir", "Toplam Hareket Geliri", "", Fs(it));
            }

            var totalCashIncome = cashIncomeTotal + ac.CashTotal + ReportContext.CurrentWorkPeriod.CashAmount;
            var totalCreditCardIncome = creditCardIncomeTotal + ac.CreditCardTotal + ReportContext.CurrentWorkPeriod.CreditCardAmount;
            var totalTicketIncome = ticketIncomeTotal + ac.TicketTotal + ReportContext.CurrentWorkPeriod.TicketAmount;

            report.AddRow("Gelir", "TOPLAMLAR", "", "");
            report.AddRow("Gelir", GetPaymentString(0), "Toplam Gelir", Fs(totalCashIncome));
            report.AddRow("Gelir", GetPaymentString(1), "Toplam Gelir", Fs(totalCreditCardIncome));
            report.AddRow("Gelir", GetPaymentString(2), "Toplam Gelir", Fs(totalTicketIncome));
            report.AddRow("Gelir", "GENEL TOPLAM", "", Fs(totalCashIncome + totalCreditCardIncome + totalTicketIncome));

            //--------------------

            report.AddColumTextAlignment("Toplam", TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Toplam", "Auto", "25*");
            report.AddTable("Toplam", "Kasa Durumu", "");
            report.AddRow("Toplam", "Kasada Bulunan Nakit", Fs(totalCashIncome - cashExpenseTotal));
            report.AddRow("Toplam", "Kasada Bulunan Kredi Kartı", Fs(totalCreditCardIncome - creditCardExpenseTotal));
            report.AddRow("Toplam", "Kasada Bulunan Yemek Çeki", Fs(totalTicketIncome - ticketExpenseTotal));
            report.AddRow("Toplam", "GENEL TOPLAM",
                Fs((totalCashIncome - cashExpenseTotal) +
                (totalCreditCardIncome - creditCardExpenseTotal) +
                (totalTicketIncome - ticketExpenseTotal)));
            return report.Document;
        }

        private static string Fct(CashTransactionData data)
        {
            var cn = !string.IsNullOrEmpty(data.CustomerName) ? data.CustomerName + " " : "";
            return data.Date.ToShortDateString() + " " + cn + data.Name;
        }

        protected override string GetHeader()
        {
            return "Kasa Raporu";
        }
    }
}
