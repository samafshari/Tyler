using Avalonia;
using Avalonia.Controls;
using Net.Essentials;
using Tyler.Services;

using Tyler.ViewModels;
using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Reactive;

namespace Tyler.Views
{
    public partial class SpriteSheetControl : UserControl
    {
        static readonly ContainerService _containerService;
        static readonly BitmapCache _cache;

        static SpriteSheetControl()
        {
            _containerService = ContainerService.Instance;
            _cache = _containerService.GetOrCreate<BitmapCache>();
        }

        public SpriteViewModel? Sprite
        {
            get => (SpriteViewModel?)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteViewModel?> SpriteProperty =
            AvaloniaProperty.Register<SpriteSheetControl, SpriteViewModel?>(nameof(Sprite));

        public SpriteSheetViewModel? SpriteSheet
        {
            get => (SpriteSheetViewModel?)GetValue(SpriteSheetProperty);
            set => SetValue(SpriteSheetProperty, value);
        }

        public static readonly AvaloniaProperty<SpriteSheetViewModel?> SpriteSheetProperty =
            AvaloniaProperty.Register<SpriteSheetControl, SpriteSheetViewModel?>(nameof(SpriteSheet));

        public SpriteSheetControl()
        {
            InitializeComponent();
            Update();
            this.GetObservable(SpriteSheetProperty).Subscribe(new AnonymousObserver<SpriteSheetViewModel?>(s => Update()));
            this.GetObservable(SpriteProperty).Subscribe(new AnonymousObserver<SpriteViewModel?>(s => Update()));
        }

        void Update()
        {
            var vm = SpriteSheet;
            if (vm == null) return;
            if (vm.Sprites == null) return;
            var bmp = _cache.Get(vm.Path);
            img.Source = bmp;
        }
    }
}
