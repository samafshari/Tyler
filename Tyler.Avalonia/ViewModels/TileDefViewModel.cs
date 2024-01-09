using Avalonia.Media.Imaging;

using Net.Essentials;

using System.Linq;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class TileDefViewModel : TinyViewModel
    {
        public WorldViewModel World { get; }

        public string DisplayName => ToString();
        public CroppedBitmap? Bitmap => Animation.Bitmap;
        public SpriteViewModel? Sprite => World.SpriteSheetsManager.GetSprite(Animation.SelectedKeyFrame?.SpriteId);

        string? _id;
        public string? Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        string? _basedOn;
        public string? BasedOn
        {
            get => _basedOn;
            set => SetProperty(ref _basedOn, value);
        }

        char _char;
        public char Char
        {
            get => _char;
            set
            {
                SetProperty(ref _char, value);
                RaisePropertyChanged(nameof(CharString));
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public string CharString
        {
            get => Char.ToString();
            set
            {
                if (value.Length > 0)
                    Char = value[0];
            }
        }

        string? _script;
        public string? Script
        {
            get => _script;
            set => SetProperty(ref _script, value);
        }

        TileAnimationViewModel _animation;
        public TileAnimationViewModel Animation
        {
            get => _animation;
            set => SetProperty(ref _animation, value);
        }

        public TileDefViewModel(WorldViewModel world, TileDef? model)
        {
            World = world;
            _animation = new TileAnimationViewModel(World, model?.Animation);
            if (model is null) return;
            Id = model.Id;
            Name = model.Name;
            BasedOn = model.BasedOn;
            Char = model.Char;
            Script = model.Script;
        }

        public static TileDefViewModel FromSpriteId(WorldViewModel world, string spriteId)
        {
            var tileDef = new TileDefViewModel(world, null);
            tileDef.Animation.AddKeyFrame(spriteId);
            return tileDef;
        }

        public TileDef Serialize()
        {
            return new TileDef
            {
                Id = Id,
                Name = Name,
                BasedOn = BasedOn,
                Char = Char,
                Script = Script,
                Animation = Animation.ToModel()
            };
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Id))
                return $"⚠ {Name ?? "NONAME"}";
            return $"{Id}";
        }
    }
}