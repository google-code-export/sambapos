using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.RuleActions;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class RuleListViewModel : EntityCollectionViewModelBase<RuleViewModel, CustomRule>
    {
        protected override RuleViewModel CreateNewViewModel(CustomRule model)
        {
            return new RuleViewModel(model);
        }

        protected override CustomRule CreateNewModel()
        {
            return new CustomRule();
        }
    }
}
