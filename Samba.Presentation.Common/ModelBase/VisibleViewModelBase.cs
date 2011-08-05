using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class VisibleViewModelBase : ViewModelBase
    {
        public abstract Type GetViewType();

        DelegateCommand<object> _closeCommand;
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand<object>(OnRequestClose)); }
        }

        internal void PublishClose()
        {
            CommonEventPublisher.PublishViewClosedEvent(this);
        }

        private void OnRequestClose(object obj)
        {
            PublishClose();
        }

        public VisibleViewModelBase CallingView { get; set; }
    }
}
