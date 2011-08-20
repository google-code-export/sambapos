using System;
using System.Linq;
using Samba.Domain.Models.RuleActions;
using Samba.Services;

namespace Samba.Presentation.Common
{
    public class ActionData
    {
        public AppAction Action { get; set; }
        public string ParameterValues { get; set; }
        public object DataObject { get; set; }

        public T GetDataValue<T>(string parameterName) where T : class
        {
            var property = DataObject.GetType().GetProperty(parameterName);
            if (property != null)
                return property.GetValue(DataObject, null) as T;
            return null;
        }

        public string GetAsString(string parameterName)
        {
            return Action.GetFormattedParameter(parameterName, DataObject, ParameterValues);
        }

        public decimal GetAsDecimal(string parameterName)
        {
            decimal result;
            decimal.TryParse(GetAsString(parameterName), out result);
            return result;
        }
    }

    public static class RuleExecutor
    {
        public static void NotifyEvent(string eventName, object dataObject)
        {
            var rules = AppServices.MainDataContext.Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules.Where(x => SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = AppServices.MainDataContext.Actions.SingleOrDefault(x => x.Id == container.AppActionId);
                    if (action != null)
                    {
                        var data = new ActionData { Action = action, DataObject = dataObject, ParameterValues = container.ParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
        }

        private static bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#')
                .Select(x => x.Split(';'))
                .ToDictionary(x => x[0], x => x[1]);

            foreach (var conditionName in conditions.Keys)
            {
                var cName = conditionName;
                var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name);
                var parameterName = parameterNames.FirstOrDefault(cName.StartsWith);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var parameterValue = dataObject.GetType().GetProperty(parameterName).GetValue(dataObject, null);
                    if (conditionName.Contains("Contains"))
                    {
                        if (!parameterValue.ToString().Contains(conditions[cName])) return false;
                    }
                }
            }

            return true;
        }
    }
}
