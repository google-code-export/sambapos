using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Cron;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Presentation.Common
{
    public static class TriggerService
    {
        static TriggerService()
        {
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TriggerExecuted, "Trigger Executed", new { TriggerName = "" });
            RuleActionTypeRegistry.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
        }

        private static readonly List<CronObject> CronObjects = new List<CronObject>();

        public static void UpdateCronObjects()
        {
            foreach (var cronObject in CronObjects)
            {
                cronObject.Stop();
                cronObject.OnCronTrigger -= OnCronTrigger;
            }
            CronObjects.Clear();
            var triggers = Dao.Query<Trigger>();
            foreach (var trigger in triggers)
            {
                var dataContext = new CronObjectDataContext
                    {
                        Object = trigger,
                        LastTrigger = trigger.LastTrigger,
                        CronSchedules = new List<CronSchedule> { CronSchedule.Parse(trigger.Expression) }
                    };
                var cronObject = new CronObject(dataContext);
                cronObject.OnCronTrigger += OnCronTrigger;
                CronObjects.Add(cronObject);
            }
            CronObjects.ForEach(x => x.Start());
        }

        private static void OnCronTrigger(CronObject cronobject)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var trigger = workspace.Single<Trigger>(x => x.Id == ((Trigger)cronobject.Object).Id);
                if (trigger != null)
                {
                    trigger.LastTrigger = DateTime.Now;
                    workspace.CommitChanges();
                    RuleExecutor.NotifyEvent(RuleEventNames.TriggerExecuted, new { TriggerName = trigger.Name });
                }
                else cronobject.Stop();
            }
        }
    }
}
