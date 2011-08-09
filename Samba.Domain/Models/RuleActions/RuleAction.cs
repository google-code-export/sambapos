using System;
using System.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.RuleActions
{
    public class RuleAction : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ActionType { get; set; }
        public string Parameter { get; set; }

        public string GetParameter(string parameterName)
        {
            var param = Parameter.Split('#').Where(x => x.StartsWith(parameterName + "=")).FirstOrDefault();
            if (!string.IsNullOrEmpty(param) && param.Contains("=")) return param.Split('=')[1];
            return "";
        }
    }
}
