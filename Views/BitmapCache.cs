using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
            _cache.Add(path, new WeakReference<BitmapSource>(newBitmap));
            return newBitmap;
        }
    }
}
