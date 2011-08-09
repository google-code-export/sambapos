using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.RuleActions
{
    public class ActionContainer : IOrderable
    {
        public ActionContainer()
        {
            
        }

        public ActionContainer(RuleAction ruleAction)
        {
            RuleActionId = ruleAction.Id;
            Name = ruleAction.Name;
        }

        public int Id { get; set; }
        public int RuleActionId { get; set; }
        public int RuleId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string UserString
        {
            get { return Name; }
        }
    }
}
