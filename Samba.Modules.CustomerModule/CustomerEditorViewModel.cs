using System;
using Samba.Domain.Models.Customers;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.CustomerModule
{
    public class CustomerEditorViewModel : EntityViewModelBase<Customer>
    {
        public CustomerEditorViewModel(Customer model)
            : base(model)
        {
        }

        public override Type GetViewType()
        {
            return typeof(CustomerEditorView);
        }

        public override string GetModelTypeString()
        {
            return "Müşteri";
        }

        public string PhoneNumber { get { return Model.PhoneNumber; } set { Model.PhoneNumber = value; } }
        public string Address { get { return Model.Address; } set { Model.Address = value; } }
        public string Note { get { return Model.Note; } set { Model.Note = value; } }
        public bool InternalAccount { get { return Model.InternalAccount; } set { Model.InternalAccount = value; } }
    }
}
