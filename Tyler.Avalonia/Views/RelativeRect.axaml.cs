using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Reactive;

using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class RelativeRect : UserControl
    {
        public SpriteViewModel? Sprite
        {
            get => (SpriteViewModel?)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteViewModel?> SpriteProperty =
            AvaloniaProperty.Register<RelativeRect, SpriteViewModel?>(nameof(Sprite));

        public RelativeRect()
        {
            InitializeComponent();
            Update();
            this.GetObservable(SpriteProperty).Subscribe(new AnonymousObserver<SpriteViewModel?>(s =>
            {
                Update();
                RegisterCallbacks();
            }));
        }

        void RegisterCallbacks()
        {
            if (Sprite != null)
            {
                Sprite.PropertyChanged -= Sprite_PropertyChanged;
                Sprite.PropertyChanged += Sprite_PropertyChanged;
            }
        }

        private void Sprite_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            Update();
        }

        void Update()
        {
            if (Sprite == null) return;
            else
            {
                var sz = Sprite.GetImageSize();
                if (sz == null) return;
                if (sz.Value.Width <= 0 || sz.Value.Height <= 0) return;

                rect.Width = (double)Sprite.Width * Width / sz.Value.Width;
                rect.Height = (double)Sprite.Height * Height / sz.Value.Height;
                rect.Margin = new Thickness(
                    (double)Sprite.X * Width / sz.Value.Width,
                    (double)Sprite.Y * Height / sz.Value.Height
                    , 0, 0);
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
            }
        }
    }
}
