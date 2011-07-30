using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;

namespace Samba.Services.Printing
{
    public class TagData
    {
        public TagData(string data, string tag)
        {
            Tag = tag;
            DataString = tag;
            StartPos = data.IndexOf(tag);

            if (!data.Contains(tag)) return;

            EndPos = StartPos + 1;

            while (data[EndPos] != '}') { EndPos++; }
            EndPos++;
            Length = EndPos - StartPos;

            if (data.Length > (StartPos + Length) && data[StartPos + Length] == ']')
            {
                EndPos++;
                while (data[StartPos] != '[')
                {
                    StartPos--;
                }
            }

            Length = EndPos - StartPos;

            DataString = data.Substring(StartPos, Length);
            Title = DataString.Trim('[', ']').Replace(Tag, "");
        }

        public string DataString { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Length { get; set; }
    }

    public static class TicketFormatter
    {
        public static string[] GetFormattedTicket(Ticket ticket, IEnumerable<TicketItem> lines, PrinterTemplate template)
        {
            if (template.MergeLines) lines = MergeLines(lines);
            var orderNo = lines.Count() > 0 ? lines.ElementAt(0).OrderNumber : 0;
            var header = ReplaceDocumentVars(template.HeaderTemplate, ticket, orderNo);
            var footer = ReplaceDocumentVars(template.FooterTemplate, ticket, orderNo);
            var lns = lines.Select(x => FormatLines(template, x)).ToArray();

            var result = header.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            result.AddRange(lns);
            result.AddRange(footer.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            return result.ToArray();
        }

        private static IEnumerable<TicketItem> MergeLines(IEnumerable<TicketItem> lines)
        {
            var group = lines.Where(x => x.Properties.Count == 0).GroupBy(x => new
                                                {
                                                    x.MenuItemId,
                                                    x.MenuItemName,
                                                    x.Voided,
                                                    x.Gifted,
                                                    x.Price,
                                                    x.PortionName,
                                                    x.PortionCount,
                                                    x.ReasonId,
                                                    x.CurrencyCode
                                                });

            var result = group.Select(x => new TicketItem
                                    {
                                        MenuItemId = x.Key.MenuItemId,
                                        MenuItemName = x.Key.MenuItemName,
                                        ReasonId = x.Key.ReasonId,
                                        Voided = x.Key.Voided,
                                        Gifted = x.Key.Gifted,
                                        Price = x.Key.Price,
                                        CreatedDateTime = x.Last().CreatedDateTime,
                                        OrderNumber = x.Last().OrderNumber,
                                        TicketId = x.Last().TicketId,
                                        PortionName = x.Key.PortionName,
                                        PortionCount = x.Key.PortionCount,
                                        CurrencyCode = x.Key.CurrencyCode,
                                        Quantity = x.Sum(y => y.Quantity)
                                    });

            result = result.Union(lines.Where(x => x.Properties.Count > 0)).OrderBy(x => x.CreatedDateTime);

            return result;
        }

        private static string ReplaceDocumentVars(string document, Ticket ticket, int orderNo)
        {
            string result = document;
            if (string.IsNullOrEmpty(document)) return "";
            int userNo = ticket.TicketItems.Count > 0 ? ticket.TicketItems[0].CreatingUserId : 0;

            result = FormatData(result, "{ADİSYON TARİH}", ticket.Date.ToShortDateString());
            result = FormatData(result, "{ADİSYON SAAT}", ticket.Date.ToShortTimeString());
            result = FormatData(result, "{TARİH}", DateTime.Now.ToShortDateString());
            result = FormatData(result, "{SAAT}", DateTime.Now.ToShortTimeString());
            result = FormatData(result, "{ADİSYON ID}", ticket.Id.ToString());
            result = FormatData(result, "{ADİSYON NO}", ticket.TicketNumber);
            result = FormatData(result, "{SİPARİŞ NO}", orderNo.ToString());
            result = FormatData(result, "{ADİSYON ETİKET}", ticket.GetTagData());
            if (result.Contains("{ADİSYON ETİKET:"))
            {
                var start = result.IndexOf("{ADİSYON ETİKET:");
                var end = result.IndexOf("}", start) + 1;
                var value = result.Substring(start, end - start);
                var tags = "";
                try
                {
                    var tag = value.Trim('{', '}').Split(':')[1];
                    tags = tag.Split(',').Aggregate(tags, (current, t) => current +
                        (!string.IsNullOrEmpty(ticket.GetTagValue(t.Trim()))
                        ? (t + ": " + ticket.GetTagValue(t.Trim()) + "\r")
                        : ""));
                    result = FormatData(result.Trim('\r'), value, tags);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, "");
                }
            }

            var userName = AppServices.MainDataContext.GetUserName(userNo);

            var title = ticket.LocationName;
            if (string.IsNullOrEmpty(ticket.LocationName))
                title = userName;

            result = FormatData(result, "{MASA GARSON}", title);
            result = FormatData(result, "{GARSON}", userName);
            result = FormatData(result, "{MASA}", ticket.LocationName);
            result = FormatData(result, "{NOT}", ticket.Note);
            result = FormatData(result, "{MÜŞTERİ ADI}", ticket.CustomerName);

            if (ticket.CustomerId > 0 && (result.Contains("{MÜŞTERİ ADRES") || result.Contains("{MÜŞTERİ TELEFON")))
            {
                var customer = Dao.SingleWithCache<Customer>(x => x.Id == ticket.CustomerId);
                result = FormatData(result, "{MÜŞTERİ ADRES}", customer.Address);
                result = FormatData(result, "{MÜŞTERİ TELEFON}", customer.PhoneNumber);
            }

            result = RemoveTag(result, "{MÜŞTERİ ADRES}");
            result = RemoveTag(result, "{MÜŞTERİ TELEFON}");

            var payment = ticket.GetPaymentAmount();
            var remaining = ticket.GetRemainingAmount();
            var discount = ticket.GetTotalDiscounts();
            var plainTotal = ticket.GetPlainSum();

            result = FormatDataIf(payment > 0, result, "{VARSA ÖDENEN}",
                "Ödenen:|" + payment.ToString("#,#0.00") + "\r\n" +
                "<J>Genel TOPLAM:|" + remaining.ToString("#,#0.00"));

            result = FormatDataIf(discount > 0, result, "{VARSA İSKONTO}",
                "Belge TOPLAMI:|" + plainTotal.ToString("#,#0.00") + "\r\n" +
                "<J>İskonto:|" + discount.ToString("#,#0.00"));
            result = FormatData(result, "{TOPLAM İKRAM}", ticket.GetTotalGiftAmount().ToString("#,#0.00"));
            result = FormatDataIf(discount < 0, result, "{VARSA DÜZELTME}", "<J>Düzeltme:|" + discount.ToString("#,#0.00"));
            result = FormatData(result, "{TOPLAM FİYAT}", ticket.GetSum().ToString("#,#0.00"));
            result = FormatData(result, "{TOPLAM ÖDENEN}", ticket.GetPaymentAmount().ToString("#,#0.00"));
            result = FormatData(result, "{TOPLAM BAKİYE}", ticket.GetRemainingAmount().ToString("#,#0.00"));

            return result;
        }

        // [Toplam:{TOPLAM BAKİYE}]

        private static string FormatData(string data, string tag, string value)
        {
            var tagData = new TagData(data, tag);
            if (!string.IsNullOrEmpty(value)) value = tagData.Title + value;
            return data.Replace(tagData.DataString, value);
        }

        private static string FormatDataIf(bool condition, string data, string tag, string value)
        {
            if (condition) return FormatData(data, tag, value);
            return RemoveTag(data, tag);
        }

        private static string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }

        private static string FormatLines(PrinterTemplate template, TicketItem ticketItem)
        {
            if (ticketItem.Gifted)
            {
                if (!string.IsNullOrEmpty(template.GiftLineTemplate))
                {
                    return template.GiftLineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
                }
                return "";
            }

            if (ticketItem.Voided)
            {
                if (!string.IsNullOrEmpty(template.VoidedLineTemplate))
                {
                    return template.VoidedLineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
                }
                return "";
            }

            if (!string.IsNullOrEmpty(template.LineTemplate))
                return template.LineTemplate.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate("", (current, s) => current + ReplaceLineVars(s, ticketItem));
            return "";
        }

        private static string ReplaceLineVars(string line, TicketItem ticketItem)
        {
            string result = line;

            if (ticketItem != null)
            {
                result = FormatData(result, "{MİKTAR}", ticketItem.Quantity.ToString("#,##.##"));
                result = FormatData(result, "{ÜRÜN}", ticketItem.MenuItemName + ticketItem.GetPortionDesc());
                result = FormatData(result, "{FİYAT}", ticketItem.Price.ToString("#,#0.00"));
                result = FormatData(result, "{TUTAR}", ticketItem.GetItemPrice().ToString("#,#0.00"));
                result = FormatData(result, "{TOPLAM TUTAR}", ticketItem.GetItemValue().ToString("#,#0.00"));
                result = FormatData(result, "{YKR}", (ticketItem.Price * 100).ToString());
                result = FormatData(result, "{HAREKET TOPLAM}", ticketItem.GetTotal().ToString("#,#0.00"));
                result = FormatData(result, "{SİPARİŞ NO}", ticketItem.OrderNumber.ToString());
                result = FormatData(result, "{NEDEN}", AppServices.MainDataContext.GetReason(ticketItem.ReasonId));
                if (result.Contains("{ÖZELLİKLER"))
                {
                    string lineFormat = result;
                    if (ticketItem.Properties.Count > 0)
                    {
                        string label = "";
                        foreach (var property in ticketItem.Properties)
                        {
                            var lineValue = FormatData(lineFormat, "{ÖZELLİKLER}", property.Name);
                            lineValue = FormatData(lineValue, "{ÖZELLİK MİKTAR}", property.Quantity.ToString("#.##"));
                            lineValue = FormatData(lineValue, "{ÖZELLİK FİYAT}", property.CalculateWithParentPrice ? "" : property.PropertyPrice.Amount.ToString("#.##"));
                            label += lineValue + "\r\n";
                        }
                        result = "\r\n" + label;
                    }
                    else result = "";
                }
            }
            return result;
        }
    }
}
