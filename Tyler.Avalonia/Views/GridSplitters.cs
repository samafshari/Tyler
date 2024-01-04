using Avalonia.Controls;
using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Views
{
    public class GridSplitterH : GridSplitter
    {
        public GridSplitterH()
        {
            Background = new SolidColorBrush(0xAAAAAA);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            Height = 2;
        }
    }

    public class GridSplitterV : GridSplitter
    {
        public GridSplitterV()
        {
            Background = new SolidColorBrush(0xAAAAAA);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            Width = 2;
        }
    }
}
