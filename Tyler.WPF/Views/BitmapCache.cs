using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Tyler.Models;

namespace Tyler.Views
{
    public class BitmapCache : Singleton<BitmapCache>
    {
        readonly Dictionary<string, WeakReference<BitmapSource>> _cache = new Dictionary<string, WeakReference<BitmapSource>>();

        public BitmapSource Get(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_cache.TryGetValue(path, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _cache.Remove(path);
            }
            var newBitmap = new BitmapImage(new Uri($"file://{path}"));
            RenderOptions.SetBitmapScalingMode(newBitmap, BitmapScalingMode.NearestNeighbor);
            _cache.Add(path, new WeakReference<BitmapSource>(newBitmap));
            return newBitmap;
        }

        public BitmapSource Get(string path, Sprite sprite)
        {
            return Get(path, sprite.X, sprite.Y, sprite.Width, sprite.Height);
        }

        public BitmapSource Get(string path, int x, int y, int w, int h)
        {
            var key = $"{path}:{x}:{y}:{w}:{h}";
            if (_cache.TryGetValue(key, out var reference))
            {
                if (reference.TryGetTarget(out var bitmap)) return bitmap;
                else _cache.Remove(key);
            }

            var bmp = Get(path);
            var cropped = new CroppedBitmap(bmp, new Int32Rect(x, y, w, h));
            RenderOptions.SetBitmapScalingMode(cropped, BitmapScalingMode.NearestNeighbor);
            _cache.Add(key, new WeakReference<BitmapSource>(cropped));
            return cropped;
        }
    }
}
