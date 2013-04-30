#region

using System;

#endregion

namespace Vermeil.Navigation
{
    public class FastResumeArgs : EventArgs
    {
        public FastResumeArgs(Uri uri)
        {
            Uri = uri;
        }
        
        public Uri Uri { get; set; }
        
        public bool PreserveLastOpenedPage { get; set; }
        
        public bool ClearHistory { get; set; }
    }
}
