using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;

namespace Samba.Services
{
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
        public string[] ParameterNames { get; set; }
        public string[] ConstraintNames { get; set; }
    }

    public static class RuleActionTypeRegistry
    {
        public static IDictionary<string, RuleEvent> RuleEvents = new Dictionary<string, RuleEvent>();

        public static string[] GetParameterNames(string eventKey)
        {
            return RuleEvents[eventKey].ParameterNames;
        }

        public static void RegisterEvent(string eventKey, string eventName)
        {
            RegisterEvent(eventKey, eventName, null, null);
        }

        public static void RegisterEvent(string eventKey, string eventName, string[] parameterNames, string[] constraintNames)
        {
            if (!RuleEvents.ContainsKey(eventKey))
                RuleEvents.Add(eventKey, new RuleEvent
                {
                    EventKey = eventKey,
                    EventName = eventName,
                    ParameterNames = parameterNames ?? new string[] { },
                    ConstraintNames = constraintNames ?? new string[] { }
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

        public static IEnumerable<string> GetEventConstraints(string eventName)
        {
            return RuleEvents[eventName].ConstraintNames;
        }
    }
}
