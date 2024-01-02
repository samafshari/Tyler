using Avalonia.Media.Imaging;
using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Avalonia;

namespace Tyler.Services
{
    public class BitmapCache
    {
        readonly Dictionary<string, WeakReference<Bitmap>> _cache = new Dictionary<string, WeakReference<Bitmap>>();
        readonly Dictionary<string, WeakReference<CroppedBitmap>> _croppedCache = new Dictionary<string, WeakReference<CroppedBitmap>>();

        public Bitmap? Get(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_cache.TryGetValue(path, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _cache.Remove(path);
            }
            var newBitmap = new Bitmap(path);
            _cache.Add(path, new WeakReference<Bitmap>(newBitmap));
            return newBitmap;
        }

        public CroppedBitmap? Get(string? path, Sprite sprite)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Get(path, sprite.X, sprite.Y, sprite.Width, sprite.Height);
        }

        public CroppedBitmap? Get(string? path, int x, int y, int w, int h)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var key = $"{path}:{x}:{y}:{w}:{h}";
            if (_croppedCache.TryGetValue(key, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _croppedCache.Remove(key);
            }

            var bmp = Get(path);
            if (bmp == null) return null;
            var cropped = new CroppedBitmap(bmp, new PixelRect(x, y, w, h));
            _croppedCache.Add(key, new WeakReference<CroppedBitmap>(cropped));
            return cropped;
        }
    }
}
