using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CairoDesktop.WindowsTasks
{
    [Serializable]
    public sealed class NotifyPropertyChangedAspect : PostSharp.Laos.OnMethodBoundaryAspect
    {
        public string PropertyName
        {
            get;
            set;
        }

        public NotifyPropertyChangedAspect(string PropertyName)
        {
            this.PropertyName = PropertyName;
        }

        public override void OnSuccess(PostSharp.Laos.MethodExecutionEventArgs eventArgs)
        {
            base.OnSuccess(eventArgs);
            var instance = (ICairoNotifyPropertyChanged)eventArgs.Instance;
            if (instance != null)
            {
                instance.OnPropertyChanged(this.PropertyName);
            }
        }
    }
}