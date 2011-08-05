using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Samba.Presentation.Common
{
    public static class ItemsControlExtensions
    {
        public static void BringItemIntoView(this ItemsControl itemsControl, object item)
        {
            var generator = itemsControl.ItemContainerGenerator;

            if (!TryBringContainerIntoView(generator, item))
            {
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    switch (generator.Status)
                    {
                        case GeneratorStatus.ContainersGenerated:
                            TryBringContainerIntoView(generator, item);
                            break;
                        case GeneratorStatus.Error:
                            generator.StatusChanged -= handler;
                            break;
                        case GeneratorStatus.GeneratingContainers:
                            return;
                        case GeneratorStatus.NotStarted:
                            return;
                        default:
                            break;
                    }
                };

                generator.StatusChanged += handler;
            }
        }

        private static bool TryBringContainerIntoView(ItemContainerGenerator generator, object item)
        {
            var container = generator.ContainerFromItem(item) as FrameworkElement;
            if (container != null)
            {
                container.BringIntoView();
                return true;
            }
            return false;
        }
    }
}
