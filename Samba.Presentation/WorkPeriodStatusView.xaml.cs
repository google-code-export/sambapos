using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for WorkPeriodStatusViewModel.xaml
    /// </summary>
    public partial class WorkPeriodStatusView : UserControl
    {
        private readonly Timer _timer;
        public WorkPeriodStatusView()
        {
            InitializeComponent();
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
            _timer = new Timer(OnTimerTick, null, 1, 60000);
        }

        private void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.WorkPeriodStatusChanged)
            {
                _timer.Change(1, 60000);
            }
        }

        private void OnTimerTick(object state)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate
            {
                try
                {
                    if (AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
                    {
                        var ts = new TimeSpan(DateTime.Now.Ticks - AppServices.MainDataContext.CurrentWorkPeriod.StartDate.Ticks);
                        tbWorkPeriodStatus.Visibility = ts.TotalHours > 24 ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else tbWorkPeriodStatus.Visibility = Visibility.Collapsed;
                }
                catch (Exception)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }

            }));
        }
    }
}
