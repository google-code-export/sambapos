using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public class RuleActionType
    {
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string[] ParameterNames { get; set; }
        public object[] ParamterValues { get; set; }
    }



    public static class RuleActionTypeRegistry
    {
        static RuleActionTypeRegistry()
        {
            RegisterActionType("ShowMessage", "Show Message", new[] { "Message" }, new[] { "" });
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
    }
}
