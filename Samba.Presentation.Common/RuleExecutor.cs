using System.Linq;
using Samba.Services;

namespace Samba.Presentation.Common
{
    public static class RuleExecutor
    {
        public static void NotifyEvent(string eventName)
        {
            var rules = AppServices.MainDataContext.Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules)
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = AppServices.MainDataContext.Actions.SingleOrDefault(x => x.Id == container.RuleActionId);
                    if (action != null)
                        action.PublishEvent(EventTopicNames.ExecuteEvent, true);
                }
            }
        }
    }
}
