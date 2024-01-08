using Avalonia.Media.Imaging;

using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SpriteKeyFrameViewModel : TinyViewModel
    {
        public WorldViewModel World { get; }

        string? _spriteId;
        public string? SpriteId
        {
            get => _spriteId;
            set
            {
                var isDirty = _spriteId != value;
                SetProperty(ref _spriteId, value);
                if (isDirty)
                    RaisePropertyChanged(nameof(Bitmap));
            }
        }

        double _duration = 1;
        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public CroppedBitmap? Bitmap => World?.GetSprite(SpriteId)?.Bitmap;

        public SpriteKeyFrameViewModel(WorldViewModel world, SpriteKeyFrame? model)
        {
            World = world;
            if (model is null) return;
            SpriteId = model.SpriteId;
            Duration = model.Duration;
        }

        public SpriteKeyFrame Serialize()
        {
            return new SpriteKeyFrame
            {
                SpriteId = SpriteId,
                Duration = Duration
            };
        }
    }
}
