using Avalonia.Media.Imaging;
using Avalonia.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Avalonia;
using System.Runtime.InteropServices;

namespace Tyler.Services
{
    public class BitmapCache
    {
        readonly Dictionary<string, WeakReference<Bitmap>> _cache = new Dictionary<string, WeakReference<Bitmap>>();
        readonly Dictionary<string, WeakReference<CroppedBitmap>> _croppedCache = new Dictionary<string, WeakReference<CroppedBitmap>>();

        readonly Bitmap _defaultBitmap;
        readonly CroppedBitmap _defaultCroppedBitmap;

        public BitmapCache()
        {
            var bmp = new RenderTargetBitmap(new PixelSize(16, 16), new Vector(96, 96));
            using (var context = bmp.CreateDrawingContext())
                context.FillRectangle(Brushes.CornflowerBlue, new Rect(0, 0, bmp.PixelSize.Width, bmp.PixelSize.Height));
            _defaultBitmap = bmp;
            _defaultCroppedBitmap = new CroppedBitmap(bmp, new PixelRect(0, 0, bmp.PixelSize.Width, bmp.PixelSize.Height));
        }

        public Bitmap Get(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return _defaultBitmap;
            if (_cache.TryGetValue(path, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _cache.Remove(path);
            }
            var newBitmap = new Bitmap(path);
            _cache.Add(path, new WeakReference<Bitmap>(newBitmap));
            return newBitmap;
        }

        public CroppedBitmap Get(string? path, Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(path)) return _defaultCroppedBitmap;
            return Get(path, sprite.X, sprite.Y, sprite.Width, sprite.Height);
        }

        public CroppedBitmap Get(string? path, int x, int y, int w, int h)
        {
            if (string.IsNullOrWhiteSpace(path)) return _defaultCroppedBitmap;

            var key = $"{path}:{x}:{y}:{w}:{h}";
            if (_croppedCache.TryGetValue(key, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _croppedCache.Remove(key);
            }

            var bmp = Get(path);
            if (bmp == _defaultBitmap) return _defaultCroppedBitmap;

            var cropped = new CroppedBitmap(bmp, new PixelRect(x, y, w, h));
            _croppedCache.Add(key, new WeakReference<CroppedBitmap>(cropped));
            return cropped;
        }
    }
}
