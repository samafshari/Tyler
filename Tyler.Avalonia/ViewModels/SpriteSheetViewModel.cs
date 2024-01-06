using Avalonia.Media.Imaging;

using Net.Essentials;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SpriteSheetViewModel : TinyViewModel
    {
        static readonly BitmapCache _bitmapCache;
        public event EventHandler<NameChangeEventArgs>? IdChanged;
        public event EventHandler<NameChangeEventArgs>? SpriteIdChanged;
        public event EventHandler<ObservableCollection<SpriteViewModel>>? SpriteListChanged;

        public SpriteSheetEditorViewModel Editor { get; }

        static SpriteSheetViewModel()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
        }

        ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();
        public ObservableCollection<SpriteViewModel> Sprites
        {
            get => _sprites;
            private set => SetProperty(ref _sprites, value);
        }

        string _path;
        public string Path
        {
            get => _path;
            set
            {
                SetProperty(ref _path, value);
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        string _id;
        public string Id
        {
            get => _id;
            set
            {
                var oldId = _id;
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
                if (oldId != value)
                    IdChanged?.Invoke(this, new NameChangeEventArgs(oldId, value));
            }
        }

        public string DisplayName => ToString();

        public Bitmap Bitmap => _bitmapCache.Get(Path);

        public SpriteSheetViewModel(string path)
        {
            _path = path;
            _id = System.IO.Path.GetFileName(path) ?? Guid.NewGuid().ToString();
            Sprites = new ObservableCollection<SpriteViewModel>();
            RegisterEvents();

            Editor = new SpriteSheetEditorViewModel(this);
        }

        public SpriteSheetViewModel(SpriteSheet model)
        {
            if (model.Path == null)
                throw new ArgumentNullException(nameof(model.Path));

            _path = model.Path;
            _id = model.Id ?? Guid.NewGuid().ToString();
            Sprites = new ObservableCollection<SpriteViewModel>(model.Sprites.Select(x => new SpriteViewModel(Path, x)));
            RegisterEvents();

            Editor = new SpriteSheetEditorViewModel(this);
        }

        void RegisterEvents()
        {
            Sprites.CollectionChanged += (s, e) =>
            {
                SpriteListChanged?.Invoke(this, Sprites);
                if (e.OldItems != null) foreach (SpriteViewModel sprite in e.OldItems)
                    sprite.IdChanged -= Sprite_IdChanged;
                if (e.NewItems != null) foreach (SpriteViewModel sprite in e.NewItems)
                    sprite.IdChanged += Sprite_IdChanged;
            };

            foreach (var sprite in Sprites)
                sprite.IdChanged += Sprite_IdChanged;
        }

        private void Sprite_IdChanged(object? sender, NameChangeEventArgs e)
        {
            SpriteIdChanged?.Invoke(this, e);
        }

        public SpriteSheet Serialize()
        {
            var model = new SpriteSheet
            {
                Id = Id,
                Path = Path,
                Sprites = Sprites.Select(x => x.Serialize()).ToList()
            };
            return model;
        }

        public SpriteViewModel AddSprite()
        {
            var id = Sprites.Count.ToString();

            if (Sprites.Any())
                id = Sprites.Select(x => int.TryParse(x.Id, out var _i) ? _i : 0).Max() + 1 + "";

            var sprite = new SpriteViewModel(Path, new Sprite
            {
                Id = id,
                Char = Vars.DefaultChar,
                X = 0,
                Y = 0,
                Width = Bitmap.PixelSize.Width,
                Height = Bitmap.PixelSize.Height
            });
            Sprites.Add(sprite);

            return sprite;
        }

        public void ClearSprites()
        {
            Sprites.Clear();
        }

        public override string ToString()
        {
            return File.Exists(Path) ? Id : $"[FNF] {Id}";
        }
    }
}
