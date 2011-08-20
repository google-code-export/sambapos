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
    public class RuleConstraintViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class RuleViewModel : EntityViewModelBase<AppRule>
    {
        private IWorkspace _workspace;

        public RuleViewModel(AppRule model)
            : base(model)
        {
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this)));

            SelectActionsCommand = new CaptionCommand<string>(Resources.SelectActions, OnSelectActions);
            if (!string.IsNullOrEmpty(model.EventConstraints))
            {
                Constraints = new ObservableCollection<RuleConstraintViewModel>(
                    model.EventConstraints.Split('#')
                    .Select(x => x.Split(';'))
                    .Select(x => new RuleConstraintViewModel { Name = x[0], Value = x[1] }));
            }
        }

        private void OnSelectActions(string obj)
        {
            IList<IOrderable> selectedValues = new List<IOrderable>(Model.Actions);
            var selectedIds = selectedValues.Select(x => ((ActionContainer)x).AppActionId);
            IList<IOrderable> values = new List<IOrderable>(_workspace.All<AppAction>(x => !selectedIds.Contains(x.Id)).Select(x => new ActionContainer(x)));

            var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, Resources.ActionList,
                                                                                   Resources.SelectActions, Resources.Action, Resources.Actions);

            foreach (var action in Model.Actions.ToList())
            {
                var laction = action;
                if (choosenValues.FirstOrDefault(x => ((ActionContainer)x).AppActionId == laction.AppActionId) == null)
                {
                    if (action.Id > 0)
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

        private ObservableCollection<RuleConstraintViewModel> _constraints;
        public ObservableCollection<RuleConstraintViewModel> Constraints
        {
            get { return _constraints; }
            set
            {
                _constraints = value;
                RaisePropertyChanged("Constraints");
            }
        }

        public IEnumerable<RuleEvent> Events { get { return RuleActionTypeRegistry.RuleEvents.Values; } }

        public ICaptionCommand SelectActionsCommand { get; set; }

        public string EventName
        {
            get { return Model.EventName; }
            set
            {
                Model.EventName = value;

                Constraints = new ObservableCollection<RuleConstraintViewModel>(
                    RuleActionTypeRegistry.GetEventConstraints(Model.EventName)
                        .Select(x => new RuleConstraintViewModel { Name = x })
                );
            }
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

        protected override void OnSave(string value)
        {
            Model.EventConstraints = string.Join("#", Constraints.Select(x => x.Name + ";" + x.Value));
            base.OnSave(value);
        }
    }
}
