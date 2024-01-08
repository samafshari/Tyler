using Avalonia.Media.Imaging;

using Net.Essentials;

using System.Collections.ObjectModel;
using System.Linq;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class TileAnimationViewModel : TinyViewModel
    {
        public WorldViewModel World { get; }

        public CroppedBitmap? Bitmap => SelectedKeyFrame?.Bitmap;

        AnimationMode _animationMode;
        public AnimationMode AnimationMode
        {
            get => _animationMode;
            set => SetProperty(ref _animationMode, value);
        }

        ObservableCollection<SpriteKeyFrameViewModel> _keyFrames = new ObservableCollection<SpriteKeyFrameViewModel>();
        public ObservableCollection<SpriteKeyFrameViewModel> KeyFrames
        {
            get => _keyFrames;
            set => SetProperty(ref _keyFrames, value);
        }

        double _rate = 1;
        public double Rate
        {
            get => _rate;
            set => SetProperty(ref _rate, value);
        }

        double _initialTimeVariance = 0;
        public double InitialTimeVariance
        {
            get => _initialTimeVariance;
            set => SetProperty(ref _initialTimeVariance, value);
        }

        SpriteKeyFrameViewModel? _selectedKeyFrame;
        public SpriteKeyFrameViewModel? SelectedKeyFrame
        {
            get => _selectedKeyFrame;
            set
            {
                _selectedKeyFrame = value;
                UpdateProperties();
            }
        }

        public TileAnimationViewModel(WorldViewModel world, TileAnimation? model)
        {
            World = world;
            if (model is null) return;
            AnimationMode = model.Mode;
            Rate = model.Rate;
            InitialTimeVariance = model.InitialTimeVariance;
            if (model.KeyFrames is not null)
            {
                foreach (var keyFrame in model.KeyFrames)
                {
                    KeyFrames.Add(new SpriteKeyFrameViewModel(World, keyFrame));
                }
                SelectedKeyFrame = KeyFrames.LastOrDefault();
            }
        }

        public static TileAnimationViewModel FromSpriteId(WorldViewModel world, string spriteId)
        {
            var tileAnimation = new TileAnimationViewModel(world, null);
            tileAnimation.AddKeyFrame(spriteId);
            return tileAnimation;
        }

        public TileAnimation ToModel()
        {
            return new TileAnimation
            {
                Mode = AnimationMode,
                Rate = Rate,
                InitialTimeVariance = InitialTimeVariance,
                KeyFrames = KeyFrames.Select(x => x.Serialize()).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{AnimationMode} ({KeyFrames.Count} frames)";
        }

        public SpriteKeyFrameViewModel AddKeyFrame()
        {
            return AddKeyFrame(new SpriteKeyFrameViewModel(World, null));
        }

        public SpriteKeyFrameViewModel AddKeyFrame(string spriteId)
        {
            return AddKeyFrame(new SpriteKeyFrameViewModel(World, new SpriteKeyFrame { SpriteId = spriteId }));
        }

        public SpriteKeyFrameViewModel AddKeyFrame(SpriteKeyFrameViewModel keyFrame)
        {
            KeyFrames.Add(keyFrame);
            SelectedKeyFrame = keyFrame;
            return keyFrame;
        }

        public void RemoveKeyFrame(SpriteKeyFrameViewModel keyFrame)
        {
            KeyFrames.Remove(keyFrame);
            if (SelectedKeyFrame == keyFrame)
                SelectedKeyFrame = KeyFrames.LastOrDefault();
        }

        public void RemoveKeyFrame()
        {
            if (SelectedKeyFrame is null) return;
            RemoveKeyFrame(SelectedKeyFrame);
        }

        public void MoveKeyFrameUp(SpriteKeyFrameViewModel keyFrame)
        {
            var index = KeyFrames.IndexOf(keyFrame);
            if (index == 0) return;
            KeyFrames.Move(index, index - 1);
        }

        public void MoveKeyFrameUp()
        {
            if (SelectedKeyFrame is null) return;
            MoveKeyFrameUp(SelectedKeyFrame);
        }

        public void MoveKeyFrameDown(SpriteKeyFrameViewModel keyFrame)
        {
            var index = KeyFrames.IndexOf(keyFrame);
            if (index == KeyFrames.Count - 1) return;
            KeyFrames.Move(index, index + 1);
        }

        public void MoveKeyFrameDown()
        {
            if (SelectedKeyFrame is null) return;
            MoveKeyFrameDown(SelectedKeyFrame);
        }

        public void ClearKeyFrames()
        {
            KeyFrames.Clear();
        }

        [Updates] public CommandModel RemoveKeyFrameCommand => new CommandModel(RemoveKeyFrame, _ => SelectedKeyFrame is not null);
        [Updates] public CommandModel MoveKeyFrameUpCommand => new CommandModel(MoveKeyFrameUp, _ => SelectedKeyFrame is not null);
        [Updates] public CommandModel MoveKeyFrameDownCommand => new CommandModel(MoveKeyFrameDown, _ => SelectedKeyFrame is not null);
        [Updates] public CommandModel ClearKeyFramesCommand => new CommandModel(ClearKeyFrames, _ => KeyFrames.Count > 0);
        [Updates] public CommandModel AddKeyFrameCommand => new CommandModel(() => AddKeyFrame());
    }
}
