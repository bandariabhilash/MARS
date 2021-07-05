using System;
using System.Collections.Generic;
using System.Threading;

namespace FarmerBrothers.Utilities
{
    public static class RecordLock
    {

        private static IList<int> ErfIds = new List<int>();
        private static object locker = new object();

        public static void AddErfId(int erfId)
        {
            lock (locker)
            {
                ErfIds.Add(erfId);
            }
        }

        public static bool IsLocked(int erfId)
        {
            return ErfIds.Contains(erfId);
        }
    }
}
