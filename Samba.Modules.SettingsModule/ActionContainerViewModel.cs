using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Domain.Models.RuleActions;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class ActionParameterValue : ObservableObject
    {
        public string Name { get; set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ActionContainer.UpdateParameters();
            }
        }

        public IEnumerable<string> ParameterValues { get; set; }

        public ActionContainerViewModel ActionContainer { get; set; }
        public ActionParameterValue(ActionContainerViewModel actionContainer, string name, string value, IEnumerable<string> parameterValues)
        {
            Name = name;
            _value = value;
            ActionContainer = actionContainer;
            ParameterValues = parameterValues.Select(x => string.Format("[{0}]", x));
        }
    }

    public class ActionContainerViewModel : ObservableObject
    {

        public ActionContainerViewModel(ActionContainer model, RuleViewModel ruleViewModel)
        {
            Model = model;
            _ruleViewModel = ruleViewModel;
        }

        private RuleViewModel _ruleViewModel;
        private AppAction _action;
        public AppAction Action { get { return _action ?? (_action = Dao.Single<AppAction>(x => x.Id == Model.AppActionId)); } set { _action = value; } }

        public ActionContainer Model { get; set; }

        private ObservableCollection<ActionParameterValue> _parameterValues;
        public ObservableCollection<ActionParameterValue> ParameterValues
        {
            get { return _parameterValues ?? (_parameterValues = GetParameterValues()); }
        }

        private ObservableCollection<ActionParameterValue> GetParameterValues()
        {
            IEnumerable<ActionParameterValue> result;
            if (string.IsNullOrEmpty(Model.ParameterValues))
            {
                result = Regex.Matches(Action.Parameter, "\\[([^\\]]+)\\]")
                    .Cast<Match>()
                    .Select(match => new ActionParameterValue(this, match.Groups[1].Value, "", RuleActionTypeRegistry.GetParameterNames(_ruleViewModel.EventName)));
            }
            else
            {
                result = Model.ParameterValues.Split('#').Select(
                x => new ActionParameterValue(this, x.Split('=')[0], x.Split('=')[1], RuleActionTypeRegistry.GetParameterNames(_ruleViewModel.EventName)));
            }

            return new ObservableCollection<ActionParameterValue>(result);
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public void UpdateParameters()
        {
            Model.ParameterValues = string.Join("#", ParameterValues.Select(x => x.Name + "=" + x.Value));
        }
    }
}
