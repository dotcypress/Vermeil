#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vermeil.Common;

#endregion

namespace Vermeil.Cache
{
    internal class ImageCache : IImageCache
    {
        public const int DefaultMemoryCacheCapacity = 100;
        private const string ImageDataExtension = "data";
        private const string ImageTimestampExtension = "tstamp";
        public static readonly TimeSpan DefaultExpirationDelay = TimeSpan.FromDays(30);
        private TimeSpan _expirationDelay = DefaultExpirationDelay;

        public ImageCache()
        {
            IsEnabled = true;
        }

        public ImageSource Get(Uri imageUri)
        {
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                throw new UnauthorizedAccessException("invalid cross-thread access");
            }
            if (!IsEnabled || imageUri == null || !imageUri.IsAbsoluteUri)
            {
                return null;
            }
            return GetInternal(imageUri);
        }

        public TimeSpan ExpirationDelay
        {
            get { return _expirationDelay; }
            set
            {
                if (value.TotalMinutes < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _expirationDelay = value;
                RequestCachePruning();
            }
        }

        public bool IsEnabled { get; set; }

        public void Cleanup()
        {
            PruneCache();
        }

        public void Clear()
        {
            lock (_storeLock)
            {
                lock (_memoryCacheLock)
                {
                    ClearMemoryCache();
                    DeleteAllImagesFromStore();
                }
            }
        }

        private ImageSource GetInternal(Uri imageUri)
        {
            var imageSource = new BitmapImage();

            var imageKey = GetImageKey(imageUri);
            var imageDataStream = LoadImageFromMemoryCache(imageKey);
            if (imageDataStream != null)
            {
                SetImageSource(imageKey, imageSource, imageDataStream);
                return imageSource;
            }

            var imageSourceRef = new WeakReference(imageSource);
            ThreadPool.QueueUserWorkItem(state => LoadImageSource(imageUri, imageSourceRef));
            return imageSource;
        }

        private void LoadImageSource(Uri imageUri, WeakReference imageSourceRef)
        {
            var imageSource = imageSourceRef.Target as BitmapImage;
            if (imageSource == null)
            {
                return;
            }

            var imageKey = GetImageKey(imageUri);
            var imageDataStream = LoadImageFromMemoryCache(imageKey) ?? ReadImageDataFromCache(imageKey);
            if (imageDataStream != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => SetImageSource(imageKey, imageSource, imageDataStream));
            }
            else
            {
                RequestImageDownload(imageUri, imageSourceRef);
            }
        }

        private void SetImageSource(string imageKey, BitmapSource imageSource, Stream imageDataStream)
        {
            if (imageSource == null)
            {
                throw new ArgumentNullException("imageSource");
            }
            try
            {
                imageSource.SetSource(imageDataStream);
            }
            catch (Exception)
            {
                DeleteImageFromCache(imageKey);
                DeleteImageFromMemoryCache(imageKey);
            }
        }

        #region Image Downloads

        private readonly Dictionary<Uri, ImageRequest> _pendingRequests = new Dictionary<Uri, ImageRequest>();

        private void RequestImageDownload(Uri imageUri, WeakReference imageSourceRef)
        {
            if (imageUri == null || imageSourceRef == null || imageSourceRef.Target == null)
            {
                return;
            }

            lock (_pendingRequests)
            {
                PrunePendingRequests();

                if (_pendingRequests.ContainsKey(imageUri))
                {
                    var request = _pendingRequests[imageUri];
                    lock (request)
                    {
                        _pendingRequests[imageUri].SourceRefs.Add(imageSourceRef);
                    }
                }
                else
                {
                    var request = new ImageRequest(imageUri);
                    request.Completed += OnImageRequestCompleted;
                    request.SourceRefs.Add(imageSourceRef);
                    _pendingRequests[imageUri] = request;
                    try
                    {
                        request.Start();
                    }
                    catch (Exception)
                    {
                        _pendingRequests.Remove(imageUri);
                    }
                }
            }
        }

        private void OnImageRequestCompleted(object sender, EventArgs e)
        {
            var request = sender as ImageRequest;
            if (request == null)
            {
                return;
            }

            lock (_pendingRequests)
            {
                PrunePendingRequests();

                if (!_pendingRequests.ContainsKey(request.ImageUri))
                {
                    return;
                }
                _pendingRequests.Remove(request.ImageUri);

                if (request.ImageData == null || request.ImageData.Length == 0)
                {
                    return;
                }

                var imageKey = GetImageKey(request.ImageUri);
                WriteImageToCache(imageKey, request.ImageData);
                WriteImageToMemoryCache(imageKey, request.ImageData);

                foreach (var sourceRef in request.SourceRefs)
                {
                    var imageSource = sourceRef.Target as BitmapImage;
                    if (imageSource == null)
                    {
                        continue;
                    }
                    Stream imageDataStream = new MemoryStream(request.ImageData);
                    Deployment.Current.Dispatcher.BeginInvoke(() => SetImageSource(imageKey, imageSource, imageDataStream));
                }
            }
        }

        private void PrunePendingRequests()
        {
            lock (_pendingRequests)
            {
                List<Uri> obsoleteUris = null;

                foreach (var imageUri in _pendingRequests.Keys)
                {
                    var request = _pendingRequests[imageUri];
                    var hasSources = request.SourceRefs.Any(sourceRef => sourceRef.Target != null);
                    if (hasSources)
                    {
                        continue;
                    }
                    if (obsoleteUris == null)
                    {
                        obsoleteUris = new List<Uri>();
                    }
                    obsoleteUris.Add(imageUri);
                }

                if (obsoleteUris != null)
                {
                    foreach (var obsoleteUri in obsoleteUris)
                    {
                        var request = _pendingRequests[obsoleteUri];
                        _pendingRequests.Remove(obsoleteUri);
                        request.Cancel();
                    }
                }
            }
        }

        #endregion

        #region Store Access

        private readonly object _storeLock = new object();

        private IsolatedStorageFile _store;

        private static string StoreDirectoryName
        {
            get { return "Vermeil.ImageCache"; }
        }

        private IsolatedStorageFile Store
        {
            get
            {
                lock (_storeLock)
                {
                    if (_store == null)
                    {
                        _store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (!_store.DirectoryExists(StoreDirectoryName))
                        {
                            _store.CreateDirectory(StoreDirectoryName);
                        }
                    }
                    return _store;
                }
            }
        }

        private static string GetImageKey(Uri imageUri)
        {
            var imageUriBytes = Encoding.UTF8.GetBytes(imageUri.ToString());
            var hash = new SHA1Managed().ComputeHash(imageUriBytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private static string GetImageFilePath(string imageKey)
        {
            return Path.Combine(StoreDirectoryName, imageKey) + "." + ImageDataExtension;
        }

        private static string GetTimestampFilePath(string imageKey)
        {
            return Path.Combine(StoreDirectoryName, imageKey) + "." + ImageTimestampExtension;
        }

        private Stream ReadImageDataFromCache(string imageKey)
        {
            RequestCachePruning();

            try
            {
                var imageFilePath = GetImageFilePath(imageKey);
                lock (_storeLock)
                {
                    if (!Store.FileExists(imageFilePath))
                    {
                        return null;
                    }
                    if (GetImageTimestamp(imageKey).Add(ExpirationDelay) < DateTime.UtcNow)
                    {
                        DeleteImageFromCache(imageKey);
                        return null;
                    }
                    return Store.OpenFile(imageFilePath, FileMode.Open, FileAccess.Read);
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            return null;
        }

        private void WriteImageToCache(string imageKey, byte[] imageData)
        {
            RequestCachePruning();

            var imageFilePath = GetImageFilePath(imageKey);
            try
            {
                lock (_storeLock)
                {
                    var fileStream = Store.OpenFile(imageFilePath,
                        Store.FileExists(imageFilePath)
                            ? FileMode.Create
                            : FileMode.CreateNew,
                        FileAccess.Write);
                    using (fileStream)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        while (fileStream.Position < imageData.Length)
                        {
                            fileStream.Write(imageData, (int) fileStream.Position, (int) (imageData.Length - fileStream.Position));
                        }
                    }
                    SetImageTimestamp(imageKey, DateTime.UtcNow);
                }
            }
            catch (Exception)
            {
                try
                {
                    Store.DeleteFile(imageFilePath);
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
        }

        private void PrunePersistentCache()
        {
            try
            {
                lock (_storeLock)
                {
                    var searchPattern = Path.Combine(StoreDirectoryName, string.Format("*.{0}", ImageDataExtension));
                    var fileNames = Store.GetFileNames(searchPattern);
                    foreach (var fileName in fileNames)
                    {
                        if (!fileName.EndsWith("." + ImageDataExtension))
                        {
                            continue;
                        }
                        var imageKey = fileName.Remove(Math.Max(fileName.Length - ImageDataExtension.Length - 1, 0));
                        if (GetImageTimestamp(imageKey).Add(ExpirationDelay) < DateTime.UtcNow)
                        {
                            DeleteImageFromCache(imageKey);
                        }
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void DeleteImageFromCache(string imageKey)
        {
            var imageFilePath = GetImageFilePath(imageKey);
            var timestampFilePath = GetTimestampFilePath(imageKey);
            lock (_storeLock)
            {
                try
                {
                    if (Store.FileExists(imageFilePath))
                    {
                        Store.DeleteFile(imageFilePath);
                    }
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                }
                try
                {
                    if (Store.FileExists(timestampFilePath))
                    {
                        Store.DeleteFile(timestampFilePath);
                    }
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
        }

        private void DeleteAllImagesFromStore()
        {
            lock (_storeLock)
            {
                var searchPattern = Path.Combine(StoreDirectoryName, "*.*");
                try
                {
                    var fileNames = Store.GetFileNames(searchPattern);
                    foreach (var fileName in fileNames)
                    {
                        var filePath = Path.Combine(StoreDirectoryName, fileName);
                        try
                        {
                            Store.DeleteFile(filePath);
                        }
// ReSharper disable EmptyGeneralCatchClause
                        catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                        {
                        }
                    }
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
        }

        private DateTime GetImageTimestamp(string imageKey)
        {
            var timestampFilePath = GetTimestampFilePath(imageKey);
            try
            {
                lock (_storeLock)
                {
                    if (!Store.FileExists(timestampFilePath))
                    {
                        return DateTime.MinValue;
                    }
                    var fileStream = Store.OpenFile(timestampFilePath, FileMode.Open, FileAccess.Read);
                    using (var fileStreamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        var timestampString = fileStreamReader.ReadToEnd();
                        return DateTime.Parse(timestampString).ToUniversalTime();
                    }
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        private void SetImageTimestamp(string imageKey, DateTime timestamp)
        {
            var timestampFilePath = GetTimestampFilePath(imageKey);
            try
            {
                lock (_storeLock)
                {
                    var fileStream = Store.OpenFile(timestampFilePath,
                        Store.FileExists(timestampFilePath)
                            ? FileMode.Create
                            : FileMode.CreateNew,
                        FileAccess.Write);
                    using (var fileStreamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        fileStreamWriter.Write(timestamp.ToUniversalTime().ToString("u"));
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        #endregion

        #region Cache Pruning

        private static readonly TimeSpan CachePruningInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan CachePruningTimerDuration = TimeSpan.FromSeconds(5);

        private OneShotDispatcherTimer _cachePruningTimer;
        private DateTime _cachePruningTimestamp = DateTime.MinValue;

        private void RequestCachePruning()
        {
            lock (this)
            {
                if (_cachePruningTimer != null || _cachePruningTimestamp.Add(CachePruningInterval) >= DateTime.UtcNow)
                {
                    return;
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (_cachePruningTimer != null)
                        {
                            return;
                        }
                        _cachePruningTimer = OneShotDispatcherTimer.CreateAndStart(CachePruningTimerDuration, OnCachePruningTimerFired);
                    });
            }
        }

        private void OnCachePruningTimerFired(object sender, EventArgs e)
        {
            if (sender != _cachePruningTimer)
            {
                return;
            }
            _cachePruningTimer = null;
            _cachePruningTimestamp = DateTime.UtcNow;
            ThreadPool.QueueUserWorkItem(state => PruneCache());
        }

        private void PruneCache()
        {
            PrunePersistentCache();
            PruneMemoryCache();
        }

        #endregion

        #region Memory Cache

        private readonly LinkedList<byte[]> _memoryCacheList = new LinkedList<byte[]>();
        private readonly object _memoryCacheLock = new object();
        private readonly Dictionary<string, LinkedListNode<byte[]>> _memoryCacheNodes = new Dictionary<string, LinkedListNode<byte[]>>(DefaultMemoryCacheCapacity);
        private int _memoryCacheCapacity = DefaultMemoryCacheCapacity;

        public int MemoryCacheCapacity
        {
            get { return _memoryCacheCapacity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                lock (_memoryCacheLock)
                {
                    _memoryCacheCapacity = value;
                    PruneMemoryCache();
                }
            }
        }

        private Stream LoadImageFromMemoryCache(string imageKey)
        {
            byte[] imageData;
            lock (_memoryCacheLock)
            {
                if (_memoryCacheCapacity == 0)
                {
                    return null;
                }
                if (!_memoryCacheNodes.ContainsKey(imageKey))
                {
                    return null;
                }
                var node = _memoryCacheNodes[imageKey];
                if (node.List == _memoryCacheList)
                {
                    _memoryCacheList.Remove(node);
                }
                _memoryCacheList.AddLast(node.Value);
                PruneMemoryCache();
                imageData = node.Value;
            }
            return new MemoryStream(imageData);
        }

        private void WriteImageToMemoryCache(string imageKey, byte[] imageData)
        {
            if (string.IsNullOrEmpty(imageKey) || imageData == null || imageData.Length == 0)
            {
                return;
            }
            lock (_memoryCacheLock)
            {
                if (_memoryCacheCapacity == 0)
                {
                    return;
                }
                if (_memoryCacheNodes.ContainsKey(imageKey))
                {
                    _memoryCacheList.Remove(_memoryCacheNodes[imageKey]);
                }
                var newNode = _memoryCacheList.AddLast(imageData);
                PruneMemoryCache();
                _memoryCacheNodes[imageKey] = newNode;
            }
        }

        private void PruneMemoryCache()
        {
            lock (_memoryCacheLock)
            {
                if (_memoryCacheCapacity == 0)
                {
                    ClearMemoryCache();
                    return;
                }
                while (_memoryCacheList.Count > _memoryCacheCapacity)
                {
                    DeleteFirstMemoryCacheNode();
                }
            }
        }

        private void DeleteFirstMemoryCacheNode()
        {
            lock (_memoryCacheLock)
            {
                var node = _memoryCacheList.First;
                if (node == null)
                {
                    return;
                }
                _memoryCacheList.Remove(node);
                foreach (var imageKey in _memoryCacheNodes.Keys)
                {
                    if (_memoryCacheNodes[imageKey] == node)
                    {
                        _memoryCacheNodes.Remove(imageKey);
                        break;
                    }
                }
            }
        }

        private void DeleteImageFromMemoryCache(string imageKey)
        {
            if (string.IsNullOrEmpty(imageKey))
            {
                return;
            }
            lock (_memoryCacheLock)
            {
                if (!_memoryCacheNodes.ContainsKey(imageKey))
                {
                    return;
                }
                var node = _memoryCacheNodes[imageKey];
                _memoryCacheNodes.Remove(imageKey);
                if (node.List == _memoryCacheList)
                {
                    _memoryCacheList.Remove(node);
                }
            }
        }

        private void ClearMemoryCache()
        {
            lock (_memoryCacheLock)
            {
                _memoryCacheNodes.Clear();
                _memoryCacheList.Clear();
            }
        }

        #endregion
    }
}
