using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ViewObjects;
using Samba.Presentation.ViewModels;
using Samba.Services;


namespace Samba.Modules.TicketModule
{
    public class TicketListViewModel : ObservableObject
    {
        private const int OpenTicketListView = 0;
        private const int SingleTicketView = 1;

        private readonly Timer _timer;

        public DelegateCommand<ScreenMenuItemData> AddMenuItemCommand { get; set; }
        public CaptionCommand<string> CloseTicketCommand { get; set; }
        public DelegateCommand<int?> OpenTicketCommand { get; set; }
        public CaptionCommand<string> MakePaymentCommand { get; set; }

        public CaptionCommand<string> MakeCashPaymentCommand { get; set; }
        public CaptionCommand<string> MakeCreditCardPaymentCommand { get; set; }
        public CaptionCommand<string> MakeTicketPaymentCommand { get; set; }
        public CaptionCommand<string> SelectTableCommand { get; set; }
        public CaptionCommand<string> SelectCustomerCommand { get; set; }
        public CaptionCommand<string> PrintTicketCommand { get; set; }
        public CaptionCommand<string> PrintInvoiceCommand { get; set; }
        public CaptionCommand<string> ShowAllOpenTickets { get; set; }
        public CaptionCommand<string> MoveTicketItemsCommand { get; set; }

        public ICaptionCommand IncQuantityCommand { get; set; }
        public ICaptionCommand DecQuantityCommand { get; set; }
        public ICaptionCommand IncSelectionQuantityCommand { get; set; }
        public ICaptionCommand DecSelectionQuantityCommand { get; set; }
        public ICaptionCommand ShowVoidReasonsCommand { get; set; }
        public ICaptionCommand ShowGiftReasonsCommand { get; set; }
        public ICaptionCommand ShowTicketTagsCommand { get; set; }
        public ICaptionCommand CancelItemCommand { get; set; }
        public ICaptionCommand ShowExtraPropertyEditorCommand { get; set; }
        public ICaptionCommand EditTicketNoteCommand { get; set; }
        public ICaptionCommand RemoveTicketLockCommand { get; set; }
        public ICaptionCommand RemoveTicketTagCommand { get; set; }
        public ICaptionCommand ChangePriceCommand { get; set; }
        public ICaptionCommand PrintJobCommand { get; set; }
        public DelegateCommand<TicketTagFilterViewModel> FilterOpenTicketsCommand { get; set; }

        private TicketViewModel _selectedTicket;
        public TicketViewModel SelectedTicket
        {
            get
            {
                if (AppServices.MainDataContext.SelectedTicket == null) _selectedTicket = null;

                if (_selectedTicket == null && AppServices.MainDataContext.SelectedTicket != null)
                    _selectedTicket = new TicketViewModel(AppServices.MainDataContext.SelectedTicket,
                        AppServices.MainDataContext.SelectedDepartment.IsFastFood);

                return _selectedTicket;
            }
        }

        private readonly ObservableCollection<TicketItemViewModel> _selectedTicketItems;
        public TicketItemViewModel SelectedTicketItem
        {
            get
            {
                return SelectedTicket != null && SelectedTicket.SelectedItems.Count == 1 ? SelectedTicket.SelectedItems[0] : null;
            }
        }

        public IEnumerable<PrintJobButton> PrintJobButtons
        {
            get
            {
                return SelectedTicket != null
                    ? SelectedTicket.PrintJobButtons.Where(x => x.Model.UseFromPos)
                    : null;
            }
        }

        public IEnumerable<Department> Departments { get { return AppServices.MainDataContext.Departments; } }
        public IEnumerable<Department> PermittedDepartments { get { return AppServices.MainDataContext.PermittedDepartments; } }

        public IEnumerable<OpenTicketView> OpenTickets { get; set; }

        private IEnumerable<TicketTagFilterViewModel> _openTicketTags;
        public IEnumerable<TicketTagFilterViewModel> OpenTicketTags
        {
            get { return _openTicketTags; }
            set
            {
                _openTicketTags = value;
                RaisePropertyChanged("OpenTicketTags");
            }
        }

        private string _selectedTag;
        public string SelectedTag
        {
            get { return !string.IsNullOrEmpty(_selectedTag) ? _selectedTag : SelectedDepartment != null ? SelectedDepartment.DefaultTag : null; }
            set { _selectedTag = value; }
        }

        private int _selectedTicketView;
        public int SelectedTicketView
        {
            get { return _selectedTicketView; }
            set
            {
                StopTimer();
                if (value == OpenTicketListView)
                {
                    AppServices.ActiveAppScreen = AppScreens.TicketList;
                    StartTimer();
                }
                if (value == SingleTicketView)
                {
                    AppServices.ActiveAppScreen = AppScreens.SingleTicket;
                }
                _selectedTicketView = value;
                RaisePropertyChanged("SelectedTicketView");
            }
        }

        public Department SelectedDepartment
        {
            get { return AppServices.MainDataContext.SelectedDepartment; }
            set
            {
                if (value != AppServices.MainDataContext.SelectedDepartment)
                {
                    AppServices.MainDataContext.SelectedDepartment = value;
                    RaisePropertyChanged("SelectedDepartment");
                    RaisePropertyChanged("SelectedTicket");
                    //DisplayTickets();
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
                }
                else
                {
                    DisplayTickets();
                }
            }
        }

        public bool IsDepartmentSelectorVisible
        {
            get
            {
                return PermittedDepartments.Count() > 1 &&
                       AppServices.IsUserPermittedFor(PermissionNames.ChangeDepartment);
            }
        }

        public bool IsItemsSelected { get { return _selectedTicketItems.Count > 0; } }
        public bool IsItemsSelectedAndUnlocked { get { return _selectedTicketItems.Count > 0 && _selectedTicketItems.Where(x => x.IsLocked).Count() == 0; } }
        public bool IsItemsSelectedAndLocked { get { return _selectedTicketItems.Count > 0 && _selectedTicketItems.Where(x => !x.IsLocked).Count() == 0; } }
        public bool IsNothingSelected { get { return _selectedTicketItems.Count == 0; } }
        public bool IsNothingSelectedAndTicketLocked { get { return _selectedTicket != null && _selectedTicketItems.Count == 0 && _selectedTicket.IsLocked; } }
        public bool IsNothingSelectedAndTicketTagged { get { return _selectedTicket != null && _selectedTicketItems.Count == 0 && SelectedTicket.IsTagged; } }
        public bool IsTicketSelected { get { return SelectedTicket != null && _selectedTicketItems.Count == 0; } }
        public bool IsTicketTotalVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketTotalVisible; } }
        public bool IsTicketPaymentVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketPaymentVisible; } }
        public bool IsTicketRemainingVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketRemainingVisible; } }
        public bool IsTicketDiscountVisible { get { return SelectedTicket != null && SelectedTicket.IsTicketDiscountVisible; } }

        public bool IsTableButtonVisible
        {
            get
            {
                return ((AppServices.MainDataContext.TableCount > 0 ||
                        (AppServices.MainDataContext.SelectedDepartment != null
                        && AppServices.MainDataContext.SelectedDepartment.IsAlaCarte))
                        && IsNothingSelected) &&
                        ((AppServices.MainDataContext.SelectedDepartment != null &&
                        AppServices.MainDataContext.SelectedDepartment.TableScreenId > 0));
            }
        }

        public bool IsCustomerButtonVisible
        {
            get
            {
                return (AppServices.MainDataContext.CustomerCount > 0 ||
                    (AppServices.MainDataContext.SelectedDepartment != null
                    && AppServices.MainDataContext.SelectedDepartment.IsTakeAway))
                    && IsNothingSelected;
            }
        }

        public bool CanChangeDepartment
        {
            get { return SelectedTicket == null && AppServices.MainDataContext.IsCurrentWorkPeriodOpen; }
        }

        public Brush TicketBackground { get { return SelectedTicket != null && (SelectedTicket.IsLocked || SelectedTicket.IsPaid) ? SystemColors.ControlLightBrush : SystemColors.WindowBrush; } }

        public int OpenTicketListViewColumnCount { get { return SelectedDepartment != null ? SelectedDepartment.OpenTicketViewColumnCount : 5; } }
        public TicketItemViewModel LastSelectedTicketItem { get; set; }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return AppServices.MainDataContext.SelectedDepartment != null
                    ? AppServices.MainDataContext.SelectedDepartment.TicketTagGroups
                    .Where(x => x.ActiveOnPosClient)
                    .OrderBy(x => x.Order)
                    .Select(x => new TicketTagButton(x, SelectedTicket))
                    : null;
            }
        }

        public TicketListViewModel()
        {
            _timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);
            _selectedTicketItems = new ObservableCollection<TicketItemViewModel>();

            PrintJobCommand = new CaptionCommand<PrintJob>("Yaz", OnPrintJobExecute, CanExecutePrintJob);

            AddMenuItemCommand = new DelegateCommand<ScreenMenuItemData>(OnAddMenuItemCommandExecute);
            CloseTicketCommand = new CaptionCommand<string>("Adisyonu\nKapat", OnCloseTicketExecute, CanCloseTicket);
            OpenTicketCommand = new DelegateCommand<int?>(OnOpenTicketExecute);
            MakePaymentCommand = new CaptionCommand<string>("Ödeme Al", OnMakePaymentExecute, CanMakePayment);
            MakeCashPaymentCommand = new CaptionCommand<string>("Nakit\nÖdeme", OnMakeCashPaymentExecute, CanMakeFastPayment);
            MakeCreditCardPaymentCommand = new CaptionCommand<string>("Kredi\nKartı", OnMakeCreditCardPaymentExecute, CanMakeFastPayment);
            MakeTicketPaymentCommand = new CaptionCommand<string>("Yemek\nÇeki", OnMakeTicketPaymentExecute, CanMakeFastPayment);
            SelectTableCommand = new CaptionCommand<string>("Masa Seç", OnSelectTableExecute, CanSelectTable);
            SelectCustomerCommand = new CaptionCommand<string>("Müşteri Seç", OnSelectCustomerExecute, CanSelectCustomer);
            ShowAllOpenTickets = new CaptionCommand<string>("Tüm\nBelgeler", OnShowAllOpenTickets);
            FilterOpenTicketsCommand = new DelegateCommand<TicketTagFilterViewModel>(OnFilterOpenTicketsExecute);

            IncQuantityCommand = new CaptionCommand<string>("+", OnIncQuantityCommand, CanIncQuantity);
            DecQuantityCommand = new CaptionCommand<string>("-", OnDecQuantityCommand, CanDecQuantity);
            IncSelectionQuantityCommand = new CaptionCommand<string>("(+)", OnIncSelectionQuantityCommand, CanIncSelectionQuantity);
            DecSelectionQuantityCommand = new CaptionCommand<string>("(-)", OnDecSelectionQuantityCommand, CanDecSelectionQuantity);
            ShowVoidReasonsCommand = new CaptionCommand<string>("İade", OnShowVoidReasonsExecuted, CanVoidSelectedItems);
            ShowGiftReasonsCommand = new CaptionCommand<string>("İkram", OnShowGiftReasonsExecuted, CanGiftSelectedItems);
            ShowTicketTagsCommand = new CaptionCommand<TicketTagGroup>("Etiket", OnShowTicketsTagExecute, CanExecuteShowTicketTags);
            CancelItemCommand = new CaptionCommand<string>("İptal", OnCancelItemCommand, CanCancelSelectedItems);
            MoveTicketItemsCommand = new CaptionCommand<string>("Böl", OnMoveTicketItems, CanMoveTicketItems);
            ShowExtraPropertyEditorCommand = new CaptionCommand<string>("Ekstra Özellik", OnShowExtraProperty, CanShowExtraProperty);
            EditTicketNoteCommand = new CaptionCommand<string>("Adisyon Notu", OnEditTicketNote, CanEditTicketNote);
            RemoveTicketLockCommand = new CaptionCommand<string>("Kilidi Kaldır", OnRemoveTicketLock, CanRemoveTicketLock);
            ChangePriceCommand = new CaptionCommand<string>("Fiyat Değiştir", OnChangePrice, CanChangePrice);

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.WorkPeriodStatusChanged)
                    {
                        RaisePropertyChanged("CanChangeDepartment");
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketItemViewModel>>().Subscribe(
                x =>
                {
                    if (SelectedTicket != null && x.Topic == EventTopicNames.SelectedItemsChanged)
                    {
                        LastSelectedTicketItem = x.Value.Selected ? x.Value : null;
                        foreach (var item in SelectedTicket.SelectedItems)
                        { item.IsLastSelected = item == LastSelectedTicketItem; }

                        SelectedTicket.PublishEvent(EventTopicNames.SelectedItemsChanged);
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagGroup>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.TagSelectedForSelectedTicket)
                    {
                        if (x.Value.Action == 1 && CanCloseTicket(""))
                            CloseTicketCommand.Execute("");
                        if (x.Value.Action == 2 && CanMakePayment(""))
                            MakePaymentCommand.Execute("");
                        else
                        {
                            RefreshVisuals();
                        }
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectedItemsChanged)
                    {
                        _selectedTicketItems.Clear();
                        _selectedTicketItems.AddRange(x.Value.SelectedItems);
                        if (x.Value.SelectedItems.Count == 0) LastSelectedTicketItem = null;
                        RaisePropertyChanged("IsItemsSelected");
                        RaisePropertyChanged("IsNothingSelected");
                        RaisePropertyChanged("IsNothingSelectedAndTicketLocked");
                        RaisePropertyChanged("IsTableButtonVisible");
                        RaisePropertyChanged("IsCustomerButtonVisible");
                        RaisePropertyChanged("IsItemsSelectedAndUnlocked");
                        RaisePropertyChanged("IsItemsSelectedAndLocked");
                        RaisePropertyChanged("IsTicketSelected");
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Customer>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.CustomerSelectedForTicket)
                    {
                        AppServices.MainDataContext.AssignCustomerToSelectedTicket(x.Value);
                        if (!string.IsNullOrEmpty(SelectedTicket.CustomerName) && SelectedTicket.Items.Count > 0)
                            CloseTicket();
                        else
                        {
                            RefreshVisuals();
                            SelectedTicketView = SingleTicketView;
                        }
                    }

                    if (x.Topic == EventTopicNames.PaymentRequestedForTicket)
                    {
                        AppServices.MainDataContext.AssignCustomerToSelectedTicket(x.Value);
                        if (!string.IsNullOrEmpty(SelectedTicket.CustomerName) && SelectedTicket.Items.Count > 0)
                            MakePaymentCommand.Execute("");
                        else
                        {
                            RefreshVisuals();
                            SelectedTicketView = SingleTicketView;
                        }

                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(
                x =>
                {
                    _selectedTicket = null;
                    if (x.Topic == EventTopicNames.TableSelectedForTicket)
                    {
                        if (!string.IsNullOrEmpty(SelectedTicket.Location))
                            CloseTicket();
                    }

                    if (x.Topic == EventTopicNames.PaymentSubmitted)
                    {
                        CloseTicket();
                    }

                    if (x.Topic == EventTopicNames.TicketSelectedFromTableList)
                    {
                        if (AppServices.MainDataContext.SelectedTicket == null)
                            OnOpenTicketExecute(x.Value.Id);
                        else
                        {
                            RefreshVisuals();
                        }
                        SelectedTicketView = SingleTicketView;
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                 x =>
                 {
                     if (x.Topic == EventTopicNames.ActivateTicketView)
                     {
                         SelectedTicketView = SelectedTicketView;
                         RefreshOpenTickets();
                         RefreshVisuals();

                     }

                     if (x.Topic == EventTopicNames.NavigateTicketView)
                     {
                         UpdateSelectedDepartment(AppServices.CurrentLoggedInUser.UserRole.DepartmentId);
                         SelectedTicketView = SelectedTicketView;
                         DisplayTickets();
                     }

                 });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (AppServices.ActiveAppScreen == AppScreens.TicketList
                        && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        RefreshOpenTickets();
                        RefreshVisuals();
                    }
                });
        }

        private bool CanExecuteShowTicketTags(TicketTagGroup arg)
        {
            return SelectedTicket == null || (SelectedTicket.Model.CanSubmit);
        }

        private void OnShowTicketsTagExecute(TicketTagGroup tagGroup)
        {
            if (SelectedTicket != null)
            {
                _selectedTicket.LastSelectedTicketTag = tagGroup;
                _selectedTicket.PublishEvent(EventTopicNames.SelectTicketTag);
            }
            else if (ShowAllOpenTickets.CanExecute(""))
            {
                SelectedTag = tagGroup.Name;
                RefreshOpenTickets();
                SelectedTicketView = OpenTicketListView;
                RaisePropertyChanged("OpenTickets");
            }
        }

        private bool CanChangePrice(string arg)
        {
            return SelectedTicket != null
                && !SelectedTicket.IsLocked
                && SelectedTicket.Model.CanSubmit
                && _selectedTicketItems.Count == 1
                && (_selectedTicketItems[0].Price == 0 || AppServices.IsUserPermittedFor(PermissionNames.ChangeItemPrice));
        }

        private void OnChangePrice(string obj)
        {
            decimal price;
            decimal.TryParse(AppServices.MainDataContext.NumeratorValue, out price);
            if (price <= 0)
            {
                InteractionService.UserIntraction.GiveFeedback("Fiyat değiştirmek için önce numaratörden sıfırdan büyük bir fiyat yazınız.");
            }
            else
            {
                _selectedTicketItems[0].Price = price;
            }
            _selectedTicket.ClearSelectedItems();
            _selectedTicket.RefreshVisuals();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetNumerator);
        }

        private bool CanExecutePrintJob(PrintJob arg)
        {
            return arg != null && SelectedTicket != null && (!SelectedTicket.IsLocked || SelectedTicket.Model.GetPrintCount(arg.Id) == 0);
        }

        private void OnPrintJobExecute(PrintJob printJob)
        {
            var message = SelectedTicket.GetPrintError();

            if (!string.IsNullOrEmpty(message))
            {
                InteractionService.UserIntraction.GiveFeedback(message);
                return;
            }

            SaveTicketIfNew();

            AppServices.PrintService.ManualPrintTicket(SelectedTicket.Model, printJob);

            if (printJob.WhenToPrint == (int)WhenToPrintTypes.Paid && !SelectedTicket.IsPaid)
                MakePaymentCommand.Execute("");
            else CloseTicket();
        }

        private void SaveTicketIfNew()
        {
            if (SelectedTicket.Id == 0 && SelectedTicket.Items.Count > 0)
            {
                var result = AppServices.MainDataContext.CloseTicket();
                AppServices.MainDataContext.OpenTicket(result.TicketId);
                _selectedTicket = null;
            }
        }

        private bool CanRemoveTicketLock(string arg)
        {
            return SelectedTicket != null && (SelectedTicket.IsLocked) &&
                   AppServices.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets);
        }

        private void OnRemoveTicketLock(string obj)
        {
            SelectedTicket.IsLocked = false;
            SelectedTicket.RefreshVisuals();
        }

        private void OnMoveTicketItems(string obj)
        {
            SelectedTicket.FixSelectedItems();
            var newTicketId = AppServices.MainDataContext.MoveTicketItems(SelectedTicket.SelectedItems.Select(x => x.Model), 0).TicketId;
            OnOpenTicketExecute(newTicketId);
        }

        private bool CanMoveTicketItems(string arg)
        {
            return SelectedTicket != null && SelectedTicket.CanMoveSelectedItems();
        }

        private bool CanEditTicketNote(string arg)
        {
            return SelectedTicket != null && !SelectedTicket.IsPaid;
        }

        private void OnEditTicketNote(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.EditTicketNote);
        }

        private bool CanShowExtraProperty(string arg)
        {
            return SelectedTicketItem != null && !SelectedTicketItem.Model.Locked && AppServices.IsUserPermittedFor(PermissionNames.ChangeExtraProperty);
        }

        private void OnShowExtraProperty(string obj)
        {
            _selectedTicket.PublishEvent(EventTopicNames.SelectExtraProperty);
        }

        private void OnDecQuantityCommand(string obj)
        {
            LastSelectedTicketItem.Quantity--;
            _selectedTicket.RefreshVisuals();
        }

        private void OnIncQuantityCommand(string obj)
        {
            LastSelectedTicketItem.Quantity++;
            _selectedTicket.RefreshVisuals();
        }

        private bool CanDecQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
                LastSelectedTicketItem.Quantity > 1 &&
                !LastSelectedTicketItem.IsLocked &&
                !LastSelectedTicketItem.IsVoided;
        }

        private bool CanIncQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
                !LastSelectedTicketItem.IsLocked &&
                !LastSelectedTicketItem.IsVoided;
        }

        private bool CanDecSelectionQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
                   LastSelectedTicketItem.Quantity > 1 &&
                   LastSelectedTicketItem.IsLocked &&
                   !LastSelectedTicketItem.IsGifted &&
                   !LastSelectedTicketItem.IsVoided;
        }

        private void OnDecSelectionQuantityCommand(string obj)
        {
            LastSelectedTicketItem.DecSelectedQuantity();
            _selectedTicket.RefreshVisuals();
        }

        private bool CanIncSelectionQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
               LastSelectedTicketItem.Quantity > 1 &&
               LastSelectedTicketItem.IsLocked &&
               !LastSelectedTicketItem.IsGifted &&
               !LastSelectedTicketItem.IsVoided;
        }

        private void OnIncSelectionQuantityCommand(string obj)
        {
            LastSelectedTicketItem.IncSelectedQuantity();
            _selectedTicket.RefreshVisuals();
        }

        private bool CanVoidSelectedItems(string arg)
        {
            if (_selectedTicket != null && !_selectedTicket.IsLocked && AppServices.IsUserPermittedFor(PermissionNames.VoidItems))
                return _selectedTicket.CanVoidSelectedItems();
            return false;
        }

        private void OnShowVoidReasonsExecuted(string obj)
        {
            _selectedTicket.PublishEvent(EventTopicNames.SelectVoidReason);
        }

        private void OnShowGiftReasonsExecuted(string obj)
        {
            _selectedTicket.PublishEvent(EventTopicNames.SelectGiftReason);
        }

        private bool CanCancelSelectedItems(string arg)
        {
            if (_selectedTicket != null)
                return _selectedTicket.CanCancelSelectedItems();
            return false;
        }

        private void OnCancelItemCommand(string obj)
        {
            _selectedTicket.CancelSelectedItems();
        }

        private bool CanGiftSelectedItems(string arg)
        {
            if (_selectedTicket != null && !_selectedTicket.IsLocked && AppServices.IsUserPermittedFor(PermissionNames.GiftItems))
                return _selectedTicket.CanGiftSelectedItems();
            return false;
        }

        private void OnTimer(object state)
        {
            if (AppServices.ActiveAppScreen == AppScreens.TicketList && OpenTickets != null)
                foreach (var openTicketView in OpenTickets)
                {
                    openTicketView.Refresh();
                }
        }

        private void OnShowAllOpenTickets(string obj)
        {
            UpdateOpenTickets(null, "");
            SelectedTicketView = OpenTicketListView;
            RaisePropertyChanged("OpenTickets");
        }

        private void OnFilterOpenTicketsExecute(TicketTagFilterViewModel obj)
        {
            if (string.IsNullOrEmpty(obj.TagValue))
            {
                UpdateOpenTickets(SelectedDepartment, "");
                OpenTickets = OpenTickets.Where(x =>
                        string.IsNullOrEmpty(x.TicketTag) ||
                        !x.TicketTag.ToLower().Contains((obj.TagGroup + ":").ToLower()));
            }
            else if (obj.TagValue == "*")
            {
                SelectedTag = obj.TagGroup;
                RefreshOpenTickets();
            }
            else
            {
                UpdateOpenTickets(SelectedDepartment, obj.TagGroup);
                OpenTickets = OpenTickets.Where(x => x.TicketTag.ToLower().Contains((obj.TagGroup + ":" + obj.TagValue).ToLower()));
            }

            RaisePropertyChanged("OpenTickets");
            SelectedTag = null;
        }

        private string _selectedTicketTitle;
        public string SelectedTicketTitle
        {
            get { return _selectedTicketTitle; }
            set { _selectedTicketTitle = value; RaisePropertyChanged("SelectedTicketTitle"); }
        }

        public void UpdateSelectedTicketTitle()
        {
            SelectedTicketTitle = SelectedTicket == null || SelectedTicket.Title.Trim() == "#" ? "Yeni Adisyon" : SelectedTicket.Title;
        }

        private void OnSelectCustomerExecute(string obj)
        {
            SelectedDepartment.PublishEvent(EventTopicNames.SelectCustomer);
        }

        private bool CanSelectCustomer(string arg)
        {
            return (SelectedTicket == null ||
                (SelectedTicket.Items.Count != 0
                && !SelectedTicket.IsLocked
                && SelectedTicket.Model.CanSubmit));
        }

        private bool CanSelectTable(string arg)
        {
            if (SelectedTicket != null && !SelectedTicket.IsLocked)
                return SelectedTicket.CanChangeTable();
            return SelectedTicket == null;
        }

        private void OnSelectTableExecute(string obj)
        {
            SelectedDepartment.PublishEvent(EventTopicNames.SelectTable);
        }

        public string SelectTableButtonCaption
        {
            get
            {
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.Table))
                    return "Masa\rDeğiştir";
                return "Masa\rSeç";
            }
        }

        public string SelectCustomerButtonCaption
        {
            get
            {
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.CustomerName))
                    return "Müşteri\rBilgileri";
                return "Müşteri\rSeç";
            }
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedTicket != null
                && (SelectedTicket.TicketPlainTotalValue > 0 || SelectedTicket.Items.Count > 0)
                && AppServices.IsUserPermittedFor(PermissionNames.MakePayment);
        }

        private void OnMakeCreditCardPaymentExecute(string obj)
        {
            AppServices.MainDataContext.PaySelectedTicket(PaymentType.CreditCard);
            CloseTicket();
        }

        private void OnMakeTicketPaymentExecute(string obj)
        {
            AppServices.MainDataContext.PaySelectedTicket(PaymentType.Ticket);
            CloseTicket();
        }

        private void OnMakeCashPaymentExecute(string obj)
        {
            AppServices.MainDataContext.PaySelectedTicket(PaymentType.Cash);
            CloseTicket();
        }

        private bool CanMakeFastPayment(string arg)
        {
            return SelectedTicket != null && SelectedTicket.TicketRemainingValue > 0 && AppServices.IsUserPermittedFor(PermissionNames.MakeFastPayment);
        }

        private bool CanCloseTicket(string arg)
        {
            return SelectedTicket == null || SelectedTicket.CanCloseTicket();
        }

        private void CloseTicket()
        {
            if (AppServices.MainDataContext.SelectedDepartment.IsFastFood && !CanCloseTicket(""))
            {
                SaveTicketIfNew();
                RefreshVisuals();
            }
            else if (CanCloseTicket(""))
                CloseTicketCommand.Execute("");
        }

        public void DisplayTickets()
        {
            if (SelectedDepartment != null)
            {
                if (SelectedDepartment.IsAlaCarte)
                {
                    if (SelectedTicket == null)
                        SelectedTicketView = OpenTicketListView;
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectTable);
                    StopTimer();
                    return;
                }

                if (SelectedDepartment.IsTakeAway)
                {
                    if (SelectedTicket == null)
                        SelectedTicketView = OpenTicketListView;
                    SelectedDepartment.PublishEvent(EventTopicNames.SelectCustomer);
                    StopTimer();
                    return;
                }

                SelectedTicketView = SelectedDepartment.IsFastFood ? SingleTicketView : OpenTicketListView;

                if (SelectedTicket != null)
                {
                    if (!SelectedDepartment.IsFastFood || SelectedTicket.TicketRemainingValue == 0 || !string.IsNullOrEmpty(SelectedTicket.Location))
                    {
                        SelectedTicket.ClearSelectedItems();
                    }

                    if (AppServices.CurrentTerminal.AutoLogout)
                        AppServices.CurrentLoggedInUser.PublishEvent(EventTopicNames.UserLoggedOut);
                }

                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
            }
            RefreshVisuals();

        }

        public bool IsFastPaymentButtonsVisible
        {
            get
            {
                if (SelectedTicket != null && SelectedTicket.IsPaid) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.Location)) return false;
                if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.CustomerName)) return false;
                if (SelectedTicket != null && SelectedTicket.IsTagged) return false;
                return SelectedDepartment != null && SelectedDepartment.IsFastFood;
            }
        }

        public bool IsCloseButtonVisible
        {
            get { return !IsFastPaymentButtonsVisible; }
        }

        public void RefreshOpenTickets()
        {
            UpdateOpenTickets(SelectedDepartment, SelectedTag);
            SelectedTag = string.Empty;
        }

        public void UpdateOpenTickets(Department department, string selectedTag)
        {
            StopTimer();

            Expression<Func<Ticket, bool>> prediction;

            if (department != null)
                prediction = x => !x.IsPaid && x.DepartmentId == department.Id;
            else
                prediction = x => !x.IsPaid;

            var shouldWrap = !SelectedDepartment.IsTakeAway;

            OpenTickets = Dao.Select(x => new OpenTicketView
            {
                Id = x.Id,
                LastOrderDate = x.LastOrderDate,
                TicketNumber = x.TicketNumber,
                LocationName = x.LocationName,
                CustomerName = x.CustomerName,
                RemainingAmount = x.RemainingAmount,
                Date = x.Date,
                WrapText = shouldWrap,
                TicketTag = x.Tag
            }, prediction).OrderBy(x => x.LastOrderDate);

            if (!string.IsNullOrEmpty(selectedTag))
            {
                var tag = selectedTag.ToLower() + ":";
                var cnt = OpenTickets.Count(x => string.IsNullOrEmpty(x.TicketTag) || !x.TicketTag.ToLower().Contains(tag));

                OpenTickets = OpenTickets.Where(x => !string.IsNullOrEmpty(x.TicketTag) && x.TicketTag.ToLower().Contains(tag));

                var opt = OpenTickets.SelectMany(x => x.TicketTag.Split('\r'))
                    .Where(x => x.ToLower().Contains(tag))
                    .Distinct()
                    .Select(x => x.Split(':')).Select(x => new TicketTagFilterViewModel { TagGroup = x[0], TagValue = x[1] }).OrderBy(x => x.TagValue).ToList();

                opt.Insert(0, new TicketTagFilterViewModel { TagGroup = selectedTag, TagValue = "*", ButtonColor = "Blue" });

                if (cnt > 0)
                    opt.Insert(0, new TicketTagFilterViewModel { Count = cnt, TagGroup = selectedTag, ButtonColor = "Red" });

                OpenTicketTags = opt.Count() > 1 ? opt : null;

                OpenTickets.ForEach(x => x.Info = x.TicketTag.Split('\r').Where(y => y.ToLower().StartsWith(tag)).Single().Split(':')[1]);
            }
            else
            {
                OpenTicketTags = null;
            }

            SelectedTag = selectedTag;

            StartTimer();
        }

        private void StartTimer()
        {
            if (AppServices.ActiveAppScreen == AppScreens.TicketList)
                _timer.Change(60000, 60000);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, 60000);
        }

        private static void OnMakePaymentExecute(string obj)
        {
            AppServices.MainDataContext.SelectedTicket.PublishEvent(EventTopicNames.MakePayment);
        }

        private void OnCloseTicketExecute(string obj)
        {
            if (SelectedTicket.Items.Count > 0 && SelectedTicket.Model.GetRemainingAmount() == 0)
            {
                var message = SelectedTicket.GetPrintError();
                if (!string.IsNullOrEmpty(message))
                {
                    SelectedTicket.ClearSelectedItems();
                    RefreshVisuals();
                    InteractionService.UserIntraction.GiveFeedback(message);
                    return;
                }
            }

            SelectedTicket.ClearSelectedItems();
            var result = AppServices.MainDataContext.CloseTicket();
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                InteractionService.UserIntraction.GiveFeedback(result.ErrorMessage);
            }
            _selectedTicket = null;
            _selectedTicketItems.Clear();
            DisplayTickets();
            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
            RefreshVisuals();
        }

        private void OnOpenTicketExecute(int? id)
        {
            _selectedTicket = null;
            _selectedTicketItems.Clear();
            AppServices.MainDataContext.OpenTicket(id.GetValueOrDefault(0));
            SelectedTicketView = SingleTicketView;
            RefreshVisuals();
            SelectedTicket.ClearSelectedItems();
            SelectedTicket.PublishEvent(EventTopicNames.SelectedTicketChanged);
        }

        private void RefreshVisuals()
        {
            UpdateSelectedTicketTitle();
            RaisePropertyChanged("SelectedTicket");
            RaisePropertyChanged("CanChangeDepartment");
            RaisePropertyChanged("IsTicketRemainingVisible");
            RaisePropertyChanged("IsTicketPaymentVisible");
            RaisePropertyChanged("IsTicketTotalVisible");
            RaisePropertyChanged("IsTicketDiscountVisible");
            RaisePropertyChanged("IsFastPaymentButtonsVisible");
            RaisePropertyChanged("IsCloseButtonVisible");
            RaisePropertyChanged("SelectTableButtonCaption");
            RaisePropertyChanged("SelectCustomerButtonCaption");
            RaisePropertyChanged("OpenTicketListViewColumnCount");
            RaisePropertyChanged("IsDepartmentSelectorVisible");
            RaisePropertyChanged("TicketBackground");
            RaisePropertyChanged("IsTableButtonVisible");
            RaisePropertyChanged("IsCustomerButtonVisible");
            RaisePropertyChanged("IsNothingSelectedAndTicketLocked");
            RaisePropertyChanged("IsNothingSelectedAndTicketTagged");
            RaisePropertyChanged("IsTicketSelected");
            RaisePropertyChanged("TicketTagButtons");
            RaisePropertyChanged("PrintJobButtons");

            if (SelectedTicketView == OpenTicketListView)
                RaisePropertyChanged("OpenTickets");
        }

        private void OnAddMenuItemCommandExecute(ScreenMenuItemData obj)
        {
            if (SelectedTicket == null)
            {
                AppServices.MainDataContext.CreateNewTicket();
                RefreshVisuals();
            }

            Debug.Assert(SelectedTicket != null);

            if (SelectedTicket.IsLocked && !AppServices.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return;

            var ti = SelectedTicket.AddNewItem(obj.ScreenMenuItem.MenuItemId, obj.Quantity, obj.ScreenMenuItem.Gift, obj.ScreenMenuItem.DefaultProperties);

            if (obj.ScreenMenuItem.AutoSelect && ti != null)
            {
                ti.ItemSelectedCommand.Execute(ti);
            }

            SelectedTicketView = SingleTicketView;

            RaisePropertyChanged("SelectedTicket");
            RaisePropertyChanged("IsTicketRemainingVisible");
            RaisePropertyChanged("IsTicketPaymentVisible");
            RaisePropertyChanged("IsTicketTotalVisible");
            RaisePropertyChanged("IsTicketDiscountVisible");
            RaisePropertyChanged("CanChangeDepartment");
            RaisePropertyChanged("TicketBackground");
            RaisePropertyChanged("IsTicketSelected");
        }

        public void UpdateSelectedDepartment(int departmentId)
        {
            RaisePropertyChanged("Departments");
            RaisePropertyChanged("PermittedDepartments");
            SelectedDepartment = departmentId > 0
                ? Departments.SingleOrDefault(x => x.Id == departmentId)
                : null;
        }
    }
}
