using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public ActionContainer(AppAction ruleAction)
        {
            AppActionId = ruleAction.Id;
            Name = ruleAction.Name;
        }

        public int Id { get; set; }
        public int AppActionId { get; set; }
        public int AppRuleId { get; set; }
        public string Name { get; set; }
        [StringLength(500)]
        public string ParameterValues { get; set; }
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        
    }
}
