using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : ModuleBase
    {
        readonly IRegionManager _regionManager;
        private readonly TicketEditorView _ticketEditorView;
        private readonly ICategoryCommand _navigateTicketCommand;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager, TicketEditorView ticketEditorView)
        {
            _navigateTicketCommand = new CategoryCommand<string>("POS", "Genel", "Images/Network.png", OnNavigateTicketCommand, CanNavigateTicket);
            _regionManager = regionManager;
            _ticketEditorView = ticketEditorView;

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, "Adisyon Kilidini Kaldırabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.RemoveTicketTag, PermissionCategories.Ticket, "Adisyon Etiketini Kaldırabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.GiftItems, PermissionCategories.Ticket, "İkram yapabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.VoidItems, PermissionCategories.Ticket, "İade alabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MoveTicketItems, PermissionCategories.Ticket, "Adisyon satırlarını taşıyabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MergeTickets, PermissionCategories.Ticket, "Adisyonları Birleştirebilir");
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOldTickets, PermissionCategories.Ticket, "Eski Adisyonları görüntüleyebilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedTicketItems, PermissionCategories.Ticket, "Kilitlenmemiş adisyon satırlarını taşıyabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, "Ekstra özellik girebilir");

            PermissionRegistry.RegisterPermission(PermissionNames.MakePayment, PermissionCategories.Payment, "Ödeme alabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MakeFastPayment, PermissionCategories.Payment, "Hızlı ödeme alabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MakeDiscount, PermissionCategories.Payment, "İskonto yapabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.RoundPayment, PermissionCategories.Payment, "Yuvarlama yapabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.FixPayment, PermissionCategories.Payment, "Yukarı düzeltme yapabilir");


            EventServiceFactory.EventService.GetEvent<GenericEvent<Customer>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.CustomerSelectedForTicket || x.Topic == EventTopicNames.PaymentRequestedForTicket)
                        ActivateTicketEditorView();
                }
                );

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.TableSelectedForTicket
                    || x.Topic == EventTopicNames.TicketSelectedFromTableList)
                    ActivateTicketEditorView();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateTicketView || x.Topic == EventTopicNames.NavigateTicketView)
                        ActivateTicketEditorView();
                });
        }

        private static bool CanNavigateTicket(string arg)
        {
            return AppServices.MainDataContext.IsCurrentWorkPeriodOpen;
        }

        private static void OnNavigateTicketCommand(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.NavigateTicketView);
        }

        private void ActivateTicketEditorView()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_ticketEditorView);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(TicketEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.UserRegion, typeof(DepartmentButtonView));
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishNavigationCommandEvent(_navigateTicketCommand);
        }
    }
}
