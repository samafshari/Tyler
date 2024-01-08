using Net.Essentials;

using System.Linq;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class TileDefViewModel : TinyViewModel
    {
        public WorldViewModel World { get; }

        string _char;
        public string Char
        {
            get => _char;
            set => SetProperty(ref _char, value);
        }

        string? _spriteId;
        public string? SpriteId
        {
            get => _spriteId;
            set
            {
                SetProperty(ref _spriteId, value);
                FindSprite();
            }
        }

        SpriteViewModel? _sprite;
        public SpriteViewModel? Sprite
        {
            get => _sprite;
            set => SetProperty(ref _sprite, value);
        }

        public TileDefViewModel() : this(new WorldViewModel(), new TileDef()) { }

        public TileDefViewModel(WorldViewModel world, TileDef model)
        {
            World = world;
            _char = model.Char.ToString();
            SpriteId = model.SpriteId;
        }

        public TileDef Serialize()
        {
            return new TileDef
            {
                Char = Char.FirstOrDefault(),
                SpriteId = SpriteId
            };
        }

        public void FindSprite()
        {
            if (string.IsNullOrWhiteSpace(SpriteId))
            {
                Sprite = null;
                return;
            }

            Sprite = World.GetSprite(SpriteId);
        }
    }
}