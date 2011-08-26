using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class TriggerListViewModel : EntityCollectionViewModelBase<TriggerViewModel, Trigger>
    {
        protected override TriggerViewModel CreateNewViewModel(Trigger model)
        {
            return new TriggerViewModel(model);
        }

        protected override Trigger CreateNewModel()
        {
            return new Trigger();
        }
    }
}
