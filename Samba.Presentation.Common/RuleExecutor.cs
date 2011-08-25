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

        public int GetAsInteger(string parameterName)
        {
            int result;
            int.TryParse(GetAsString(parameterName), out result);
            return result;
        }
    }

    public static class RuleExecutor
    {
        public static void NotifyEvent(string eventName, object dataObject)
        {
            var rules = AppServices.MainDataContext.Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
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
                .Select(x => new RuleConstraintViewModel(x));
            
            var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name);

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var property = dataObject.GetType().GetProperty(parameterName);

                    var parameterValue = property.GetValue(dataObject, null) ?? "";

                    if (IsNumericType(property.PropertyType))
                    {
                        var propertyValue = Convert.ToDecimal(parameterValue);
                        var objectValue = Convert.ToDecimal(condition.Value);

                        if (condition.Operation.Contains("Equals"))
                        {
                            if (!propertyValue.Equals(objectValue)) return false;
                        }
                        else if (condition.Operation.Contains("NotEquals"))
                        {
                            if (propertyValue.Equals(objectValue)) return false;
                        }
                        else if (condition.Operation.Contains("Greater"))
                        {
                            if (propertyValue < objectValue) return false;
                        }
                        else if (condition.Operation.Contains("Less"))
                        {
                            if (propertyValue > objectValue) return false;
                        }
                    }
                    else
                    {
                        var propertyValue = parameterValue.ToString().ToLower();
                        var objectValue = condition.Value.ToString().ToLower();

                        if (condition.Operation.Contains("Contains"))
                        {
                            if (!propertyValue.Contains(objectValue)) return false;
                        }
                        else if (condition.Operation.Contains("Equals"))
                        {
                            if (!propertyValue.Equals(objectValue)) return false;
                        }
                        else if (condition.Operation.Contains("NotEquals"))
                        {
                            if (propertyValue.Equals(objectValue)) return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;

        }
    }
}
