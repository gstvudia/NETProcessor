using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Processor.Core.Helpers
{
    public static class Utils
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}
