using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VsExtension.Shell
{
    public interface ILogger
    {
        void WriteText(string text);
    }

    public class OutputWindowLogger : ILogger
    {
        public Guid OutputPaneGuid = new Guid("932151e4-de47-4948-b243-5ab21cb417ef");

        private IServiceProvider _serviceProvider;

        private IVsOutputWindowPane _pane;

        private IVsOutputWindowPane Pane
        {
            get
            {
                if (_pane == null)
                    _pane = GetOrCreatePane();

                return _pane;
            }
        }
        
        public OutputWindowLogger(Package package)
        {
            _serviceProvider = package;
        }

        public void ActivateWindow()
        {
            Pane.Activate();
            Pane.Clear();
        }

        public void WriteText(string text)
        {
            Pane.OutputString(text + Environment.NewLine);
        }

        private IVsOutputWindowPane GetOrCreatePane()
        {
            IVsOutputWindow output = (IVsOutputWindow)_serviceProvider.GetService(typeof(SVsOutputWindow));
            IVsOutputWindowPane pane;

            output.GetPane(ref OutputPaneGuid, out pane);
            if (pane != null)
                return pane;

            output.CreatePane(ref OutputPaneGuid, "Inlining Analyzer", Convert.ToInt32(true), Convert.ToInt32(true));

            output.GetPane(ref OutputPaneGuid, out pane);
            return pane;
        }
    }
}
