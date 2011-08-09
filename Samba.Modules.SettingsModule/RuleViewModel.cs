using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.RuleActions;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    class RuleViewModel : EntityViewModelBase<CustomRule>
    {
        private IWorkspace _workspace;

        public RuleViewModel(CustomRule model)
            : base(model)
        {
            SelectActionsCommand = new CaptionCommand<string>("Select Actions", OnSelectActions);
        }

        private void OnSelectActions(string obj)
        {
            IList<IOrderable> selectedValues = new List<IOrderable>(Model.Actions);
            var selectedIds = selectedValues.Select(x => ((ActionContainer)x).RuleActionId);
            IList<IOrderable> values = new List<IOrderable>(_workspace.All<RuleAction>(x => !selectedIds.Contains(x.Id)).Select(x => new ActionContainer(x)));

            var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, "Action List",
                                                                                   "Select Actions", "Action", "Actions");

            foreach (var action in Model.Actions.ToList())
            {
                ActionContainer laction = action;
                if (choosenValues.FirstOrDefault(x => ((ActionContainer)x).RuleActionId == laction.RuleActionId) == null)
                {
                    _workspace.Delete(action);
                }
            }

            Model.Actions.Clear();
            foreach (ActionContainer choosenValue in choosenValues)
            {
                Model.Actions.Add(choosenValue);
            }

            RaisePropertyChanged("Actions");

        }

        public ObservableCollection<ActionContainer> Actions { get { return new ObservableCollection<ActionContainer>(Model.Actions); } }
        public IEnumerable<RuleEvent> Events { get { return RuleActionTypeRegistry.RuleEvents.Values; } }

        public ICaptionCommand SelectActionsCommand { get; set; }
        public string EventName
        {
            get { return Model.EventName; }
            set { Model.EventName = value; }
        }

        public override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
            base.Initialize(workspace);
        }

        public override Type GetViewType()
        {
            return typeof(RuleView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Rule;
        }
    }
}
