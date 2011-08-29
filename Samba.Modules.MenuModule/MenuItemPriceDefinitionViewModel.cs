using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class MenuItemPriceDefinitionViewModel : EntityViewModelBase<MenuItemPriceDefinition>
    {
        public MenuItemPriceDefinitionViewModel(MenuItemPriceDefinition model)
            : base(model)
        {
        }

        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public override Type GetViewType()
        {
            return typeof(MenuItemPriceDefinitionView);
        }

        public override string GetModelTypeString()
        {
            return "Price Definition";
        }
    }
}
