using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.CreditCardModule.ExternalProcessor
{
    [Export(typeof(ICreditCardProcessor))]
    class ExternalCreditCardProcessor : ICreditCardProcessor
    {
        private readonly ExternalProcessorViewModel _viewModel;
        private ExternalProcessorView _view;

        [ImportingConstructor]
        public ExternalCreditCardProcessor(IRegionManager regionManager, ExternalProcessorViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.Processed += ViewModelProcessed;
        }

        public string Name
        {
            get { return "External Credit Card Processor"; }
        }

        public void EditSettings()
        {
            InteractionService.UserIntraction.GiveFeedback("This processor have no setting.");
        }

        public void Process(CreditCardProcessingData creditCardProcessingData)
        {
            InteractionService.UserIntraction.BlurMainWindow();
            _viewModel.TenderedAmount = creditCardProcessingData.TenderedAmount;
            _viewModel.AuthCode = "";
            _view = new ExternalProcessorView(_viewModel);
            _view.ShowDialog();
        }

        void ViewModelProcessed(object sender, OnProcessedArgs args)
        {
            InteractionService.UserIntraction.DeblurMainWindow();
            _view.Close();

            var result = new CreditCardProcessingResult
            {
                IsCompleted = !args.Cancelled,
                Amount = _viewModel.TenderedAmount
            };

            result.PublishEvent(EventTopicNames.PaymentProcessed);
        }
    }
}
