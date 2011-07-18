using System.Windows.Controls;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Presentation.Common
{
    public static class CommonEventPublisher
    {
        public static void PublishDashboardCommandEvent(ICategoryCommand command)
        {
            command.PublishEvent(EventTopicNames.DashboardCommandAdded);
        }

        public static void PublishNavigationCommandEvent(ICategoryCommand command)
        {
            command.PublishEvent(EventTopicNames.NavigationCommandAdded);
        }

        public static void PublishViewAddedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewAdded);
        }

        public static void PublishViewClosedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewClosed);
        }

        public static void PublishDashboardUnloadedEvent(UserControl userControl)
        {
            userControl.PublishEvent(EventTopicNames.DashboardClosed);
        }
    }
}
