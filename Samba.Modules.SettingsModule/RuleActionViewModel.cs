using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.RuleActions;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    internal class ParameterValue
    {
        public string Name { get; set; }
        public string NameStr { get { return Name + ":"; } }
        public string Value { get; set; }
    }

    class RuleActionViewModel : EntityViewModelBase<RuleAction>
    {
        public RuleActionViewModel(RuleAction model)
            : base(model)
        {

        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                if (string.IsNullOrEmpty(Model.Parameter)) return new Dictionary<string, string>();
                return Model.Parameter.Split('#').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            }
        }

        public string SelectedActionType
        {
            get { return Model.ActionType; }
            set
            {
                Model.ActionType = value;
                ParameterValues = CreateParameterValues(value);
                RaisePropertyChanged("IsParameterLabelVisible");
            }
        }

        public bool IsParameterLabelVisible { get { return ParameterValues.Count > 0; } }

        private List<ParameterValue> CreateParameterValues(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<ParameterValue>();

            var result = CreateParemeterValues(RuleActionTypeRegistry.ActionTypes[value]).ToList();


            result.ForEach(x =>
                                        {
                                            if (Parameters.ContainsKey(x.Name))
                                                x.Value = Parameters[x.Name];
                                        });
            return result;
        }

        private List<ParameterValue> _parameterValues;
        public List<ParameterValue> ParameterValues
        {
            get { return _parameterValues ?? (_parameterValues = CreateParameterValues(Model.ActionType)); }
            set
            {
                _parameterValues = value;
                RaisePropertyChanged("ParameterValues");
            }
        }

        public IEnumerable<RuleActionType> ActionTypes { get { return RuleActionTypeRegistry.ActionTypes.Values; } }

        private static IEnumerable<ParameterValue> CreateParemeterValues(RuleActionType actionType)
        {
            return actionType.ParameterNames.Select(x => new ParameterValue { Name = x });
        }

        protected override void OnSave(string value)
        {
            base.OnSave(value);
            Model.Parameter = string.Join("#", ParameterValues.Select(x => x.Name + "=" + x.Value));
        }

        public override Type GetViewType()
        {
            return typeof(RuleActionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.RuleAction;
        }
    }
}
