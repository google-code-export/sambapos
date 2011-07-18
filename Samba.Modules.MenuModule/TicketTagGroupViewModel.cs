using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.MenuModule
{
    public class TicketTagGroupViewModel : EntityViewModelBase<TicketTagGroup>
    {
        private IWorkspace _workspace;

        private readonly IList<string> _actions = new[] { "Yenile", "Belgeyi Kapat", "Ödeme Al" };
        public IList<string> Actions { get { return _actions; } }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators
        {
            get { return _numerators ?? (_numerators = _workspace.All<Numerator>()); }
        }

        private readonly ObservableCollection<TicketTagViewModel> _ticketTags;
        public ObservableCollection<TicketTagViewModel> TicketTags { get { return _ticketTags; } }

        public TicketTagViewModel SelectedTicketTag { get; set; }
        public ICaptionCommand AddTicketTagCommand { get; set; }
        public ICaptionCommand DeleteTicketTagCommand { get; set; }

        public string Action { get { return Actions[Model.Action]; } set { Model.Action = Actions.IndexOf(value); } }
        public Numerator Numerator { get { return Model.Numerator; } set { Model.Numerator = value; } }
        public bool FreeTagging { get { return Model.FreeTagging; } set { Model.FreeTagging = value; } }
        public bool ForceValue { get { return Model.ForceValue; } set { Model.ForceValue = value; } }
        public bool NumericTags { get { return Model.NumericTags; } set { Model.NumericTags = value; } }
        public string ButtonColorWhenTagSelected { get { return Model.ButtonColorWhenTagSelected; } set { Model.ButtonColorWhenTagSelected = value; } }
        public string ButtonColorWhenNoTagSelected { get { return Model.ButtonColorWhenNoTagSelected; } set { Model.ButtonColorWhenNoTagSelected = value; } }
        public bool ActiveOnPosClient { get { return Model.ActiveOnPosClient; } set { Model.ActiveOnPosClient = value; } }
        public bool ActiveOnTerminalClient { get { return Model.ActiveOnTerminalClient; } set { Model.ActiveOnTerminalClient = value; } }

        public TicketTagGroupViewModel(TicketTagGroup model)
            : base(model)
        {
            _ticketTags = new ObservableCollection<TicketTagViewModel>(GetTicketTags(model));
            AddTicketTagCommand = new CaptionCommand<string>("Etiket Ekle", OnAddTicketTagExecuted);
            DeleteTicketTagCommand = new CaptionCommand<string>("Etiket Sil", OnDeleteTicketTagExecuted, CanDeleteTicketTag);
        }

        private static IEnumerable<TicketTagViewModel> GetTicketTags(TicketTagGroup ticketTagGroup)
        {
            return ticketTagGroup.TicketTags.Select(item => new TicketTagViewModel(item));
        }

        public override string GetModelTypeString()
        {
            return "Adisyon Etiketi";
        }

        public override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public override Type GetViewType()
        {
            return typeof(TicketTagGroupView);
        }

        private bool CanDeleteTicketTag(string arg)
        {
            return SelectedTicketTag != null;
        }

        private void OnDeleteTicketTagExecuted(string obj)
        {
            if (SelectedTicketTag == null) return;
            if (SelectedTicketTag.Model.Id > 0)
                _workspace.Delete(SelectedTicketTag.Model);
            Model.TicketTags.Remove(SelectedTicketTag.Model);
            TicketTags.Remove(SelectedTicketTag);
        }

        private void OnAddTicketTagExecuted(string obj)
        {
            var ti = new TicketTag { Name = "Yeni Etiket" };
            _workspace.Add(ti);
            Model.TicketTags.Add(ti);
            TicketTags.Add(new TicketTagViewModel(ti));
        }

        protected override string GetSaveErrorMessage()
        {
            if (NumericTags)
            {
                foreach (var ticketTag in TicketTags)
                {
                    try
                    {
                        Convert.ToInt32(ticketTag.Model.Name);
                    }
                    catch (Exception)
                    {
                        return "\"Sayısal etiketleme\" seçildiğinde etiketlerin tümü sayısal olmalıdır.";
                    }
                }
            }
            return base.GetSaveErrorMessage();
        }
    }
}
