using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;

using Net.Essentials;
using System.IO;
using Tyler.Services;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class SpriteControl : Image
    {
        public SpriteViewModel? Sprite
        {
            get => (SpriteViewModel?)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteViewModel?> SpriteProperty =
            AvaloniaProperty.Register<SpriteControl, SpriteViewModel?>(nameof(Sprite));

        public SpriteControl()
        {
            InitializeComponent();
            this.Bind(SourceProperty, new Binding("Sprite.Bitmap") { Source = this });
        }
    }
}