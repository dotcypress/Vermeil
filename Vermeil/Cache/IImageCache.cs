#region

using System;
using System.Windows.Media;

#endregion

namespace Vermeil.Cache
{
    public interface IImageCache
    {
        TimeSpan ExpirationDelay { get; set; }
        int MemoryCacheCapacity { get; set; }
        ImageSource Get(Uri imageUri);
        void Cleanup();
        void Clear();
    }
}
