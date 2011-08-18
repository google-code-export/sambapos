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
            return DataObject.GetType().GetProperty(parameterName).GetValue(DataObject, null) as T;
        }

        public string GetAsString(string parameterName)
        {
            return Action.GetParameter(parameterName);
        }

        public decimal GetAsDecimal(string parameterName)
        {
            decimal result;
            decimal.TryParse(Action.GetParameter(parameterName), out result);
            return result;
        }
    }

    public static class RuleExecutor
    {
        public static void NotifyEvent(string eventName, object dataObject)
        {
            var rules = AppServices.MainDataContext.Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules)
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
    }
}
