using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Samba.Infrastructure.Data;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class EntityViewModelBase<TModel> : VisibleViewModelBase where TModel : class, IEntity
    {
        public TModel Model { get; private set; }
        public ICaptionCommand SaveCommand { get; private set; }
        private readonly ValidatorFactory _validatorFactory;

        protected EntityViewModelBase(TModel model)
        {
            Model = model;
            SaveCommand = new CaptionCommand<string>("Kaydet", OnSave, CanSave);
            _validatorFactory = EnterpriseLibraryContainer.Current.GetInstance<ValidatorFactory>();
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value.Trim(); RaisePropertyChanged("Name"); }
        }

        public abstract string GetModelTypeString();

        public virtual void Initialize(IWorkspace workspace)
        {
        }

        protected override string GetHeaderInfo()
        {
            if (Model.Id > 0)
                return GetModelTypeString() + " (" + Name + ") Düzenle";
            return "Yeni " + GetModelTypeString() + " Ekle";
        }

        protected virtual void OnSave(string value)
        {
            var errorMessage = GetSaveErrorMessage();
            if (string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(Name) && CanSave(""))
            {
                if (Model.Id == 0)
                {
                    this.PublishEvent(EventTopicNames.AddedModelSaved);
                }

                this.PublishEvent(EventTopicNames.ModelAddedOrDeleted);
                ((VisibleViewModelBase)this).PublishEvent(EventTopicNames.ViewClosed);
            }
            else
            {
                if (string.IsNullOrEmpty(Name))
                    errorMessage = GetModelTypeString() + " adını boş bırakmayın.";
                MessageBox.Show(errorMessage, "Kayıt Engellendi");
            }
        }

        protected virtual string GetSaveErrorMessage()
        {
            return "";
        }

        protected virtual bool CanSave(string arg)
        {
            return Validate();
        }

        private bool Validate()
        {
            var results = _validatorFactory.CreateValidator(typeof(TModel)).Validate(Model);
            Error = GetErrors(results);
            return results.IsValid;
        }

        private static string GetErrors(IEnumerable<ValidationResult> results)
        {
            var builder = new StringBuilder();
            foreach (var result in results)
            {
                builder.AppendLine(
                    string.Format(
                       CultureInfo.CurrentCulture,
                       "* {0}",
                       result.Message));
            }
            return builder.ToString();
        }

        private string _error;
        public string Error { get { return _error; } set { _error = value; RaisePropertyChanged("Error"); } }
    }
}
