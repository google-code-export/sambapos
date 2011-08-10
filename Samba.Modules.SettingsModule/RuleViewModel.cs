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
    public class RuleViewModel : EntityViewModelBase<AppRule>
    {
        private IWorkspace _workspace;

        public RuleViewModel(AppRule model)
            : base(model)
        {
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this)));
            SelectActionsCommand = new CaptionCommand<string>("Select Actions", OnSelectActions);
        }

        private void OnSelectActions(string obj)
        {
            IList<IOrderable> selectedValues = new List<IOrderable>(Model.Actions);
            var selectedIds = selectedValues.Select(x => ((ActionContainer)x).AppActionId);
            IList<IOrderable> values = new List<IOrderable>(_workspace.All<AppAction>(x => !selectedIds.Contains(x.Id)).Select(x => new ActionContainer(x)));

            var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, "Action List",
                                                                                   "Select Actions", "Action", "Actions");

            foreach (var action in Model.Actions.ToList())
            {
                ActionContainer laction = action;
                if (choosenValues.FirstOrDefault(x => ((ActionContainer)x).AppActionId == laction.AppActionId) == null)
                {
                    _workspace.Delete(action);
                }
            }

            Model.Actions.Clear();
            choosenValues.Cast<ActionContainer>().ToList().ForEach(x => Model.Actions.Add(x));
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this)));

            RaisePropertyChanged("Actions");

        }

        private ObservableCollection<ActionContainerViewModel> _actions;
        public ObservableCollection<ActionContainerViewModel> Actions
        {
            get { return _actions; }
        }

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
