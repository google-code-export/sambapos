using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class TriggerViewModel : EntityViewModelBase<Trigger>
    {
        public TriggerViewModel(Trigger model)
            : base(model)
        {

        }

        public string Expression
        {
            get { return Model.Expression; }
            set { Model.Expression = value; }
        }

        public DateTime LastTrigger { get { return Model.LastTrigger; } set { Model.LastTrigger = value; } }

        public override Type GetViewType()
        {
            return typeof(TriggerView);
        }

        public override string GetModelTypeString()
        {
            return "Trigger";
        }

        protected override void OnSave(string value)
        {
            LastTrigger = DateTime.Now;
            base.OnSave(value);
            TriggerService.UpdateCronObjects();
        }
    }
}
