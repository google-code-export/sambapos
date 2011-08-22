using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Samba.Localization.Properties;

namespace Samba.Services
{
    public class RuleConstraintViewModel
    {
        public RuleConstraintViewModel()
        {

        }

        public RuleConstraintViewModel(string constraintData)
        {
            var parts = constraintData.Split(';');
            Name = parts[0];
            Operation = parts[1];
            if (parts.Count() > 2)
                Value = parts[2];
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public string Operation { get; set; }
        public string[] Operations { get; set; }

        public string GetConstraintData()
        {
            return Name + ";" + Operation + ";" + Value;
        }
    }

    public class RuleActionType
    {
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string[] ParameterNames { get; set; }
        public object[] ParamterValues { get; set; }
    }

    public class RuleEvent
    {
        public string EventKey { get; set; }
        public string EventName { get; set; }
        public object ParameterObject { get; set; }
    }

    public static class RuleActionTypeRegistry
    {
        public static IDictionary<string, RuleEvent> RuleEvents = new Dictionary<string, RuleEvent>();

        public static IEnumerable<string> GetParameterNames(string eventKey)
        {
            return RuleEvents[eventKey].ParameterObject.GetType().GetProperties().Select(x => x.Name);
        }

        public static void RegisterEvent(string eventKey, string eventName)
        {
            RegisterEvent(eventKey, eventName, null);
        }

        public static void RegisterEvent(string eventKey, string eventName, object constraintObject)
        {
            if (!RuleEvents.ContainsKey(eventKey))
                RuleEvents.Add(eventKey, new RuleEvent
                {
                    EventKey = eventKey,
                    EventName = eventName,
                    ParameterObject = constraintObject
                });
        }

        public static IDictionary<string, RuleActionType> ActionTypes = new Dictionary<string, RuleActionType>();
        public static void RegisterActionType(string actionType, string actionName, string[] paramterNames, object[] parameterValues)
        {
            if (!ActionTypes.ContainsKey(actionType))
                ActionTypes.Add(actionType, new RuleActionType
                                                {
                                                    ActionName = actionName,
                                                    ActionType = actionType,
                                                    ParameterNames = paramterNames,
                                                    ParamterValues = parameterValues
                                                });
        }

        public static IEnumerable<RuleConstraintViewModel> GetEventConstraints(string eventName)
        {
            var obj = RuleEvents[eventName].ParameterObject;
            if (obj != null)
            {
                var result = obj.GetType().GetProperties().Select(
                    x => new RuleConstraintViewModel { Name = x.Name, Operation = "Equals", Operations = GetOperations(x, obj) });
                return result;
            }
            return new List<RuleConstraintViewModel>();
        }

        private static string[] GetOperations(PropertyInfo propertyInfo, object o)
        {
            if (propertyInfo.GetValue(o, null) is decimal)
            {
                return new[] { "Equals", "NotEquals", "Greater", "Less" };
            }
            return new[] { "Equals", "NotEquals", "Contains" };
        }
    }
}
