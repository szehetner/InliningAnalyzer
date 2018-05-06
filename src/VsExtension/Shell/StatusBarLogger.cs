using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Shell
{
    public class StatusBarLogger
    {
        private IServiceProvider _serviceProvider;

        private readonly IVsStatusbar _statusBar;

        public StatusBarLogger(Package package)
        {
            _serviceProvider = package;

            _statusBar = (IVsStatusbar)_serviceProvider.GetService(typeof(SVsStatusbar));
        }

        public void StartProgressAnimation()
        {
            object icon = (short)Constants.SBAI_General;

            _statusBar.Animation(1, ref icon);
        }

        public void SetText(string text)
        {
            _statusBar.IsFrozen(out int frozen);
            if (frozen == 0)
            {
                _statusBar.SetText(text);
            }
        }

        public void Clear()
        {
            _statusBar.Clear();
        }

        public void StopProgressAnimation()
        {
            object icon = (short)Constants.SBAI_General;

            _statusBar.Animation(0, ref icon);
        }
    }
}
