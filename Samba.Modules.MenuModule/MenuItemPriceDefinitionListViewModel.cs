using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class MenuItemPriceDefinitionListViewModel : EntityCollectionViewModelBase<MenuItemPriceDefinitionViewModel, MenuItemPriceDefinition>
    {
        protected override MenuItemPriceDefinitionViewModel CreateNewViewModel(MenuItemPriceDefinition model)
        {
            return new MenuItemPriceDefinitionViewModel(model);
        }

        protected override MenuItemPriceDefinition CreateNewModel()
        {
            return new MenuItemPriceDefinition();
        }
    }
}
