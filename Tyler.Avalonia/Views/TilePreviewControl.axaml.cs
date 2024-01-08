using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class TilePreviewControl : UserControl
    {
        public TileViewModel? Tile
        {
            get => (TileViewModel?)GetValue(TileProperty);
            set => SetValue(TileProperty, value);
        }

        public static readonly AvaloniaProperty<TileViewModel?> TileProperty =
            AvaloniaProperty.Register<TilePreviewControl, TileViewModel?>(nameof(Tile));

        public WorldViewModel? World
        {
            get => (WorldViewModel?)GetValue(WorldProperty);
            set => SetValue(WorldProperty, value);
        }

        public static readonly AvaloniaProperty<WorldViewModel?> WorldProperty =
            AvaloniaProperty.Register<TilePreviewControl, WorldViewModel?>(nameof(World));

        public TilePreviewControl()
        {
            InitializeComponent();
            Update();
            this.GetObservable(TileProperty).Subscribe(new AnonymousObserver<TileViewModel?>(_ => Update()));
            this.GetObservable(WorldProperty).Subscribe(new AnonymousObserver<WorldViewModel?>(_ => Update()));
        }

        void Update()
        {
            if (Tile == null || World == null) sprite.Sprite = null;
            else sprite.Sprite = World.GetTileDef(Tile.Char)?.Sprite;
        }
    }
}
