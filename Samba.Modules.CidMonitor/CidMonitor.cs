using System;
using System.Linq;
using Axcidv5callerid;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Customers;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.CidMonitor
{
    [ModuleExport(typeof(CidMonitor))]
    public class CidMonitor : ModuleBase
    {
        public CidMonitor()
        {
            try
            {
                var frmMain = new FrmMain();
                frmMain.axCIDv51.OnCallerID += axCIDv51_OnCallerID;
                frmMain.axCIDv51.Start();
            }
            catch (Exception)
            {
                InteractionService.UserIntraction.DisplayPopup("Bilgi", "Caller ID Özelliğini kullanabilmek için Caller ID kutusunun sürücüsünü yüklemelisiniz.", "", "");
            }
        }

        static void axCIDv51_OnCallerID(object sender, ICIDv5Events_OnCallerIDEvent e)
        {
            var pn = e.phoneNumber;
            pn = pn.TrimStart('+');
            pn = pn.TrimStart('0');
            pn = pn.TrimStart('9');
            pn = pn.TrimStart('0');

            var c = Dao.Query<Customer>(x => x.PhoneNumber == pn);
            if (c.Count() == 0)
                c = Dao.Query<Customer>(x => x.PhoneNumber.Contains(pn));
            if (c.Count() == 1)
            {
                var customer = c.First();
                InteractionService.UserIntraction.DisplayPopup(customer.Name, customer.Name + " Arıyor.\r" + customer.PhoneNumber + "\r" + customer.Address + "\r" + customer.Note,
                                                            customer.PhoneNumber, "SelectCustomer");
            }
            else
                InteractionService.UserIntraction.DisplayPopup(e.phoneNumber, e.phoneNumber + " Arıyor...",
                                                               e.phoneNumber, "SelectCustomer");
        }
    }
}
