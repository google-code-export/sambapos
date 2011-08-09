using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.RuleActions
{
    public class CustomRule : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EventName { get; set; }

        private IList<ActionContainer> _actions;
        public virtual IList<ActionContainer> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        public CustomRule()
        {
            _actions = new List<ActionContainer>();
        }
    }
}
