using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.CacheManager
{

    public class MAICacheManager : MemoryCacheManager
    {
        public static class TypeNames
        {
            public const string PART_ORDER_MNF = "PART_ORDER_MNF";
            public const string PART_ORDER_SKU = "PART_ORDER_SKU";
            public const string CLOSER_MNF = "CLOSER_MNF";
            public const string CLOSER_SKU = "CLOSER_SKU";
            public const string EVESRCH = "EVESRCH";
            public const string ROLE_MAINTENANCE = "ROLE_MAINTENANCE";
            public const string PRIVILEGE = "PRIVILEGE";
            public const string APPUSER = "APPUSER";
            public const string TECHBRANCH = "TECHBRANCH";
            public const string TECHREGION = "TECHREGION";
            public const string TECHFSM = "TECHFSM";
            public const string PRIMARYTECH = "PRIMARYTECH";
            public const string WOTYPE = "WOTYPE";
            public const string CITIES = "CITIES";

            public const string BILLING_CLOSER_SKU = "BILLING_CLOSER_SKU";

        }
        public const string TYPE_PREFIX = "TYPE";

        #region Type Caching

        /// <summary>
        /// Gets the particular data from the cache using the type that is passed
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object getType(string type)
        {
            return Get(type, TYPE_PREFIX);
        }

        /// <summary>
        /// Adds the master data to the cache using the parameters passed
        /// </summary>
        /// <param name="type">Key to the type data</param>
        /// <param name="data">Data object</param>
        public static void setType(string type, object data)
        {
            Set(type, TYPE_PREFIX, data);
        }

        /// <summary>
        /// Checks whether a particular data is present in the cache
        /// </summary>
        /// <param name="type">Key to the type data</param>
        /// <returns></returns>
        public static bool hasType(string type)
        {
            return Has(type, TYPE_PREFIX);
        }

        /// <summary>
        /// Removes a particular master data from the cache
        /// </summary>
        /// <param name="type"></param>
        public static void removeType(string type)
        {
            Remove(type, TYPE_PREFIX);
        }

        #endregion
    }
}

