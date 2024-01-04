using Avalonia.Controls;
using Avalonia.Interactivity;

using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class WorldEditorWindow : Window
    {
        public WorldEditorWindow()
        {
            InitializeComponent();
        }

        void btnRefreshBoardPreview_Click(object sender, RoutedEventArgs e)
        {
            boardPreviewControl.Update();
        }

        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox t && t.DataContext is TileViewModel tile)
            {
                if (tile.Script != t.Text)
                    tile.Script = t.Text;
            }
        }
    }
}
