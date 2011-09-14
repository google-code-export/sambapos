using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public static class GenericRuleRegistator
    {
        private static bool _registered;
        public static void RegisterOnce()
        {
            Debug.Assert(_registered == false);
            RegisterActions();
            RegisterRules();
            RegisterParameterSources();
            HandleEvents();
            RegisterNotifiers();
            _registered = true;
        }

        private static void RegisterActions()
        {
            RuleActionTypeRegistry.RegisterActionType("SendEmail", Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false });
            RuleActionTypeRegistry.RegisterActionType("AddTicketDiscount", Resources.AddTicketDiscount, new { DiscountPercentage = 0m });
            RuleActionTypeRegistry.RegisterActionType("AddTicketItem", Resources.AddTicketItem, new { MenuItemName = "", PortionName = "", Quantity = 0, Gift = false, Tag = "" });
            RuleActionTypeRegistry.RegisterActionType("VoidTicketItems", Resources.VoidTicketItems, new { MenuItemName = "", Tag = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdateTicketTag", Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdatePriceTag", Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            RuleActionTypeRegistry.RegisterActionType("RefreshCache", Resources.RefreshCache);
            RuleActionTypeRegistry.RegisterActionType("SendMessage", Resources.BroadcastMessage, new { Command = "" });
            RuleActionTypeRegistry.RegisterActionType("UpdateProgramSetting", "Update Program Setting", new { SettingName = "", SettingValue = "" });
        }

        private static void RegisterRules()
        {
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { UserName = "", RoleName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { UserName = "", RoleName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted, new { UserName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded, new { UserName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated);
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.CustomerSelectedForTicket, Resources.CustomerSelectedForTicket, new { CustomerName = "", PhoneNumber = "", CustomerNote = "" });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { TicketTotal = 0m, PreviousTotal = 0m, DiscountTotal = 0m, GiftTotal = 0m, DiscountAmount = 0m, TipAmount = 0m });
            RuleActionTypeRegistry.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
        }

        private static void RegisterParameterSources()
        {
            RuleActionTypeRegistry.RegisterParameterSoruce("UserName", () => AppServices.MainDataContext.Users.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("DepartmentName", () => AppServices.MainDataContext.Departments.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("TerminalName", () => AppServices.Terminals.Select(x => x.Name));
            RuleActionTypeRegistry.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            RuleActionTypeRegistry.RegisterParameterSoruce("MenuItemName", () => Dao.Select<MenuItem, string>(yz => yz.Name, y => y.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("PriceTag", () => Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0));
            RuleActionTypeRegistry.RegisterParameterSoruce("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
        }

        private static void ResetCache()
        {
            TriggerService.UpdateCronObjects();
            AppServices.ResetCache();
            AppServices.MainDataContext.SelectedDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
            {
                if (x.Value.Action.ActionType == "UpdateProgramSetting")
                {
                    var settingName = x.Value.GetAsString("SettingName");
                    var settingValue = x.Value.GetAsString("SettingValue");
                    if (!string.IsNullOrEmpty(settingName))
                        AppServices.SettingService.GetCustomSetting(settingName).StringValue = settingValue;
                }

                if (x.Value.Action.ActionType == "RefreshCache")
                {
                    MethodQueue.Queue("ResetCache", ResetCache);
                }

                if (x.Value.Action.ActionType == "SendMessage")
                {
                    AppServices.MessagingService.SendMessage("ActionMessage", x.Value.GetAsString("Command"));
                }

                if (x.Value.Action.ActionType == "SendEmail")
                {
                    EMailService.SendEMailAsync(x.Value.GetAsString("SMTPServer"),
                        x.Value.GetAsString("SMTPUser"),
                        x.Value.GetAsString("SMTPPassword"),
                        x.Value.GetAsInteger("SMTPPort"),
                        x.Value.GetAsString("ToEMailAddress"),
                        x.Value.GetAsString("FromEMailAddress"),
                        x.Value.GetAsString("Subject"),
                        x.Value.GetAsString("EMailMessage"),
                        x.Value.GetAsString("FileName"),
                        x.Value.GetAsBoolean("DeleteFile"));
                }

                if (x.Value.Action.ActionType == "AddTicketDiscount")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var percentValue = x.Value.GetAsDecimal("DiscountPercentage");
                        ticket.AddTicketDiscount(DiscountType.Percent, percentValue, AppServices.CurrentLoggedInUser.Id);
                        TicketViewModel.RecalculateTicket(ticket);
                    }
                }

                if (x.Value.Action.ActionType == "AddTicketItem")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");

                    if (ticket != null)
                    {
                        var menuItemName = x.Value.GetAsString("MenuItemName");
                        var menuItem = AppServices.DataAccessService.GetMenuItemByName(menuItemName);
                        var portionName = x.Value.GetAsString("PortionName");
                        var quantity = x.Value.GetAsDecimal("Quantity");
                        var gifted = x.Value.GetAsBoolean("Gift");
                        var tag = x.Value.GetAsString("Tag");

                        var ti = ticket.AddTicketItem(AppServices.CurrentLoggedInUser.Id, menuItem, portionName,
                                 AppServices.MainDataContext.SelectedDepartment.PriceTag, "");

                        ti.Quantity = quantity;
                        ti.Gifted = gifted;
                        ti.Tag = tag;

                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "VoidTicketItems")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var menuItemName = x.Value.GetAsString("MenuItemName");
                        var tag = x.Value.GetAsString("Tag");
                        if (!string.IsNullOrEmpty(menuItemName) && !string.IsNullOrEmpty(tag))
                        {
                            var lines = ticket.TicketItems.Where(y => !y.Voided &&
                                (string.IsNullOrEmpty(menuItemName) || y.MenuItemName.Contains(menuItemName)) &&
                                (y.Tag.Contains(tag) || string.IsNullOrEmpty(tag))).ToList();
                            lines.ForEach(y =>
                                              {
                                                  ticket.VoidItem(y, 0, AppServices.CurrentLoggedInUser.Id);
                                              });
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketTag")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var tagName = x.Value.GetAsString("TagName");
                        var tagValue = x.Value.GetAsString("TagValue");
                        ticket.SetTagValue(tagName, tagValue);
                        var tagData = new TicketTagData { TagName = tagName, TagValue = tagValue };
                        tagData.PublishEvent(EventTopicNames.TagSelectedForSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "UpdatePriceTag")
                {
                    using (var workspace = WorkspaceFactory.Create())
                    {
                        var priceTag = x.Value.GetAsString("PriceTag");
                        var departmentName = x.Value.GetAsString("DepartmentName");
                        var department = workspace.Single<Department>(y => y.Name == departmentName);
                        if (department != null)
                        {
                            department.PriceTag = priceTag;
                            workspace.CommitChanges();
                            MethodQueue.Queue("ResetCache", ResetCache);
                        }
                    }
                }
            });
        }

        private static void RegisterNotifiers()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == "ActionMessage")
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }

                if (x.Topic == EventTopicNames.UserLoggedOut)
                {
                    RuleExecutor.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = x.Value, UserName = x.Value.Name, RoleName = x.Value.UserRole.Name });
                }
            });
        }
    }
}
