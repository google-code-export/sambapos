using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Samba.Presentation.Common;

namespace Samba.Modules.CreditCardModule.ExternalProcessor
{
    public class OnProcessedArgs
    {
        public bool Cancelled { get; set; }
    }

    [Export]
    public class ExternalProcessorViewModel : ObservableObject
    {
        public delegate void OnProcessed(object sender, OnProcessedArgs args);
        public event OnProcessed Processed;

        private void InvokeProcessed(OnProcessedArgs args)
        {
            OnProcessed handler = Processed;
            if (handler != null) handler(this, args);
        }

        [ImportingConstructor]
        public ExternalProcessorViewModel()
        {
            ProcessCommand = new DelegateCommand(OnProcess);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnCancel()
        {
            InvokeProcessed(new OnProcessedArgs { Cancelled = true });
        }

        private void OnProcess()
        {
            InvokeProcessed(new OnProcessedArgs { Cancelled = false });
        }

        public DelegateCommand ProcessCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public decimal TenderedAmount { get; set; }
        public string AuthCode { get; set; }
        public string CardholderName { get; set; }
    }

}
