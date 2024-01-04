using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;

using Net.Essentials;
using System.IO;
using Tyler.Services;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class SpriteControl : Image
    {
        static BitmapCache? _cache;

        SpriteViewModel? _oldSprite;
        public SpriteViewModel? Sprite
        {
            get => (SpriteViewModel?)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteViewModel?> SpriteProperty =
            AvaloniaProperty.Register<SpriteControl, SpriteViewModel?>(nameof(Sprite));

        public SpriteControl()
        {
            if (_cache == null)
                _cache = ContainerService.Instance.GetOrCreate<BitmapCache>();

            InitializeComponent();
            Update();
            this.GetObservable(SpriteProperty).Subscribe(new AnonymousObserver<SpriteViewModel?>(s => Update()));
        }

        void Update()
        {
            if (Sprite == null || !File.Exists(Sprite.Path)) Source = null;
            else if (Sprite != _oldSprite)
            {
                _oldSprite = Sprite;
                Source = _cache?.Get(Sprite.Path, Sprite.X, Sprite.Y, Sprite.Width, Sprite.Height);
            }
        }
    }
}