using Avalonia.Controls;

using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class BoardsEditorControl : UserControl
    {
        public BoardsEditorControl()
        {
            InitializeComponent();
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
