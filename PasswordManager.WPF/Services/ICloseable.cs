using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.WPF.Services
{
    public interface ICloseable
    {
        event EventHandler<CloseRequestedEventArgs> CloseRequested;
    }
    public class CloseRequestedEventArgs : EventArgs
    {
        public bool? DialogResult { get; set; }
    }
}
