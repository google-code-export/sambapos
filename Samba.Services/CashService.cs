using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain;
using Samba.Domain.Models.Cashes;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class CashTransactionData
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int PaymentType { get; set; }
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
    }

    public class CashService
    {
        public dynamic GetCurrentCashOperationData()
        {
            if (AppServices.MainDataContext.CurrentWorkPeriod == null)
                return new[] { 0m, 0m, 0m };

            var startDate = AppServices.MainDataContext.CurrentWorkPeriod.StartDate;

            var cashAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.Cash &&
                                                 x.Date > startDate);

            var creditCardAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.CreditCard &&
                                                 x.Date > startDate);

            var ticketAmount = Dao.Sum<Payment>(x => x.Amount,
                                                 x =>
                                                 x.PaymentType == (int)PaymentType.Ticket &&
                                                 x.Date > startDate);

            return new[] { cashAmount, creditCardAmount, ticketAmount };
        }

        public void AddIncome(int customerId, decimal amount, string description, PaymentType paymentType)
        {
            AddTransaction(customerId, amount, description, paymentType, TransactionType.Income);
        }

        public void AddExpense(int customerId, decimal amount, string description, PaymentType paymentType)
        {
            AddTransaction(customerId, amount, description, paymentType, TransactionType.Expense);
        }

        public IEnumerable<CashTransaction> GetTransactions(WorkPeriod workPeriod)
        {
            Debug.Assert(workPeriod != null);
            if (workPeriod.StartDate == workPeriod.EndDate)
                return Dao.Query<CashTransaction>(x => x.Date >= workPeriod.StartDate);
            return Dao.Query<CashTransaction>(x => x.Date >= workPeriod.StartDate && x.Date < workPeriod.EndDate);
        }

        public IEnumerable<CashTransactionData> GetTransactionsWithCustomerData(WorkPeriod workPeriod)
        {
            var wp = new WorkPeriod() { StartDate = workPeriod.StartDate, EndDate = workPeriod.EndDate };
            if (wp.StartDate == wp.EndDate) wp.EndDate = DateTime.Now;
            using (var workspace = WorkspaceFactory.CreateReadOnly())
            {
                var lines = from ct in workspace.Queryable<CashTransaction>()
                            join customer in workspace.Queryable<Customer>() on ct.CustomerId equals customer.Id into ctC
                            from customer in ctC.DefaultIfEmpty(Customer.Null)
                            where ct.Date >= wp.StartDate && ct.Date < wp.EndDate
                            select new CashTransactionData
                                       {
                                           Amount = ct.Amount,
                                           CustomerName = customer.Name,
                                           Date = ct.Date,
                                           Name = ct.Name,
                                           PaymentType = ct.PaymentType,
                                           TransactionType = ct.TransactionType
                                       };
                return lines.ToList();
            }
        }

        private static void AddTransaction(int customerId, decimal amount, string description, PaymentType paymentType, TransactionType transactionType)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var c = new CashTransaction
                            {
                                Amount = amount,
                                Date = DateTime.Now,
                                Name = description,
                                PaymentType = (int)paymentType,
                                TransactionType = (int)transactionType,
                                UserId = AppServices.CurrentLoggedInUser.Id,
                                CustomerId = customerId
                            };
                workspace.Add(c);
                workspace.CommitChanges();
            }
        }
    }


}
