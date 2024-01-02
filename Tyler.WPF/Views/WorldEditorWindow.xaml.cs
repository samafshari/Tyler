using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Tyler.ViewModels;

namespace Tyler.Views
{
    /// <summary>
    /// Interaction logic for WorldEditorWindow.xaml
    /// </summary>
    public partial class WorldEditorWindow
    {
        public WorldEditorWindow()
        {
            InitializeComponent();
        }

        private void btnRefreshBoardPreview_Click(object sender, RoutedEventArgs e)
        {
            boardPreviewControl.Update();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox t && t.DataContext is TileViewModel tile)
            {
                if (tile.Script != t.Text)
                    tile.Script = t.Text;
            }
        }
    }
}
