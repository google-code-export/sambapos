using System;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class PrinterTemplateViewModel : EntityViewModelBase<PrinterTemplate>
    {
        public PrinterTemplateViewModel(PrinterTemplate model)
            : base(model)
        {
        }

        public string HeaderTemplate { get { return Model.HeaderTemplate; } set { Model.HeaderTemplate = value; } }
        public string LineTemplate { get { return Model.LineTemplate; } set { Model.LineTemplate = value; } }
        public string VoidedLineTemplate { get { return Model.VoidedLineTemplate; } set { Model.VoidedLineTemplate = value; } }
        public string GiftLineTemplate { get { return Model.GiftLineTemplate; } set { Model.GiftLineTemplate = value; } }
        public string FooterTemplate { get { return Model.FooterTemplate; } set { Model.FooterTemplate = value; } }
        public bool MergeLines { get { return Model.MergeLines; } set { Model.MergeLines = value; } }

        public override Type GetViewType()
        {
            return typeof(PrinterTemplateView);
        }
        
        public override string GetModelTypeString()
        {
            return Resources.PrinterTemplate;
        }

        public override void Initialize(IWorkspace workspace)
        {
            
        }
    }
}
