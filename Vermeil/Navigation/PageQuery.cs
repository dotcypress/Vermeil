#region

using System.Collections.Generic;
using System.Linq;
using System.Net;

#endregion

namespace Vermeil.Navigation
{
    public class PageQuery : Dictionary<string, string>
    {
        public string BuildQuery()
        {
            return "?" + BuildPostQuery();
        }

        public string BuildPostQuery()
        {
            var query = string.Empty;
            if (Count > 0)
            {
                query = this.Aggregate(string.Empty, (current, keyValue) => current + string.Format("{0}={1}&", keyValue.Key, HttpUtility.UrlEncode(keyValue.Value)));
                query = query.Remove(query.Length - 1, 1);
            }
            return query;
        }

        public static string BuildQuery(PageQuery query)
        {
            return (query ?? new PageQuery()).BuildQuery();
        }
    }
}