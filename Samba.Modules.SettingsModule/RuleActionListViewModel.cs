using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.RuleActions;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class RuleActionListViewModel: EntityCollectionViewModelBase<RuleActionViewModel, RuleAction>
    {
        protected override RuleActionViewModel CreateNewViewModel(RuleAction model)
        {
            return new RuleActionViewModel(model);
        }

        protected override RuleAction CreateNewModel()
        {
            return new RuleAction();
        }
    }
}
