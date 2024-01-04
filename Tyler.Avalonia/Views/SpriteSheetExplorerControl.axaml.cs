using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Collections.Generic;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class SpriteSheetExplorerControl : UserControl
    {
        public IEnumerable<SpriteViewModel>? Sprites
        {
            get => (IEnumerable<SpriteViewModel>?)GetValue(SpritesProperty);
            set => SetValue(SpritesProperty, value);
        }

        public static readonly AvaloniaProperty<IEnumerable<SpriteViewModel>?> SpritesProperty =
            AvaloniaProperty.Register<SpriteSheetExplorerControl, IEnumerable<SpriteViewModel>?>(nameof(Sprites));

        public SpriteViewModel? SelectedSprite
        {
            get => (SpriteViewModel?)GetValue(SelectedSpriteProperty);
            set => SetValue(SelectedSpriteProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteViewModel?> SelectedSpriteProperty =
            AvaloniaProperty.Register<SpriteSheetExplorerControl, SpriteViewModel?>(nameof(SelectedSprite));

        public SpriteSheetExplorerControl()
        {
            InitializeComponent();
            grd.DataContext = this;
        }
    }
}
