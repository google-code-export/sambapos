using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Foundation
{
    public class CurrencyValue
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime CurrencyValueDate { get; set; }
        public decimal DefaultCurrencyValue { get; set; }
    }

    public class CurrencyContext : IEntity
    {

        public string UserString
        {
            get { return Name; }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] LastUpdateTime { get; set; }

        public static string DefaultCurrency { get; set; }

        private static CurrencyContext _currencyContext;
        public static CurrencyContext DefaultContext { get { return _currencyContext ?? (_currencyContext = new CurrencyContext("TL")); } }

        public string Currency { get; set; }

        public List<string> ChangeCurrencies { get; set; }
        public List<CurrencyValue> CurrencyValues { get; set; }

        public CurrencyContext()
        {
            ChangeCurrencies = new List<string>();
            CurrencyValues = new List<CurrencyValue>();
        }

        public CurrencyContext(string defaultCurrency)
            : this()
        {
            DefaultCurrency = defaultCurrency;
            Currency = defaultCurrency;
        }

        public void AddChangeCurency(string currency)
        {
            ChangeCurrencies.Add(currency);
        }

        public void AddChangeValue(DateTime currencyValueDate, string currencyCode, decimal defaultCurrencyValue)
        {
            CurrencyValues.Add(new CurrencyValue
                                   {
                                       DefaultCurrencyValue = defaultCurrencyValue,
                                       CurrencyCode = currencyCode,
                                       CurrencyValueDate = currencyValueDate
                                   });
        }

        private decimal GetCurrencyValue(DateTime date, string currencyCode)
        {
            return CurrencyValues
                .Where(x => x.CurrencyCode == currencyCode && x.CurrencyValueDate < DateTime.Now)
                .OrderByDescending(x => x.CurrencyValueDate)
                .First()
                .DefaultCurrencyValue;
        }

        public decimal ConvertTo(string currencyCode, decimal currencyValue)
        {
            decimal cv = GetCurrencyValue(DateTime.Now, currencyCode);
            return currencyValue * cv;
        }

        public decimal ConvertTo(string currencyCode, decimal currencyValue, string toCurrencyCode)
        {
            if (currencyCode == toCurrencyCode)
                return currencyValue;

            decimal cv = GetCurrencyValue(DateTime.Now, currencyCode);
            decimal cv1 = GetCurrencyValue(DateTime.Now, toCurrencyCode);
            return (currencyValue * cv) / cv1;
        }
    }
}
