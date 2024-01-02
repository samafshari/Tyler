using ModernWpf.Controls.Primitives;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tyler.Views
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            SetValue(WindowHelper.UseModernWindowStyleProperty, true);
        }
    }
}
