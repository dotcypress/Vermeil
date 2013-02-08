#region

using System;
using System.Windows.Media;

#endregion

namespace Vermeil.Cache
{
    public interface IImageCache
    {
        ImageSource Get(Uri imageUri);
        TimeSpan ExpirationDelay { get; set; }
        int MemoryCacheCapacity { get; set; }
        void Cleanup();
        void Clear();
    }
}