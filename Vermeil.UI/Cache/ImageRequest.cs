#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

#endregion

namespace Vermeil.Cache
{
    internal class ImageRequest
    {
        private bool _started;
        private HttpWebRequest _webRequest;

        public ImageRequest(Uri imageUri)
        {
            ImageUri = imageUri;
            ImageData = null;
            SourceRefs = new List<WeakReference>();
        }

        public Uri ImageUri { get; private set; }

        public byte[] ImageData { get; private set; }

        public IList<WeakReference> SourceRefs { get; private set; }

        public void Start()
        {
            lock (this)
            {
                if (_started)
                {
                    return;
                }
                _started = true;

                _webRequest = (HttpWebRequest) WebRequest.Create(ImageUri);
                _webRequest.BeginGetResponse(OnGotResponse, null);
            }
        }

        public void Cancel()
        {
            lock (this)
            {
                if (!_started)
                {
                    return;
                }
                var webRequest = _webRequest;
                _webRequest = null;
                if (webRequest != null)
                {
                    try
                    {
                        webRequest.Abort();
                    }
                        // ReSharper disable EmptyGeneralCatchClause
                    catch
                        // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                }
            }
        }

        public event EventHandler Completed;

        private void OnGotResponse(IAsyncResult asyncResult)
        {
            lock (this)
            {
                if (_webRequest == null)
                {
                    return;
                }
                try
                {
                    var webResponse = (HttpWebResponse) _webRequest.EndGetResponse(asyncResult);
                    using (var responseInputStream = webResponse.GetResponseStream())
                    {
                        var responseDataCapacity = 4096;
                        if (responseInputStream.Length >= 1 && responseInputStream.Length < Int32.MaxValue)
                        {
                            responseDataCapacity = (int) responseInputStream.Length;
                        }
                        using (var responseDataStream = new MemoryStream(responseDataCapacity))
                        {
                            var responseBuffer = new byte[4096];
                            while (true)
                            {
                                var readCount = responseInputStream.Read(responseBuffer, 0, responseBuffer.Length);
                                if (readCount <= 0)
                                {
                                    break;
                                }
                                responseDataStream.Write(responseBuffer, 0, readCount);
                            }
                            if (responseDataStream.Length > 0)
                            {
                                ImageData = responseDataStream.ToArray();
                            }
                            NotifyCompletion();
                        }
                    }
                }
                catch (Exception)
                {
                    NotifyCompletion();
                }
            }
        }

        private void NotifyCompletion()
        {
            lock (this)
            {
                _webRequest = null;

                ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (Completed == null)
                        {
                            return;
                        }
                        try
                        {
                            Completed(this, EventArgs.Empty);
                        }
                            // ReSharper disable EmptyGeneralCatchClause
                        catch (Exception)
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                        }
                    });
            }
        }
    }
}
