using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepwell.Common.Enum;

namespace Deepwell.Common.Helpers
{
    public static class Utility
    {
        public static Dictionary<InventoryLocation, string> Locations =>
            new Dictionary<InventoryLocation, string>
            {
                {InventoryLocation.One, "Location 1" },
                {InventoryLocation.Two, "Location 2" }
            };

        public static string GetLocationName(InventoryLocation location)
        {
            return GetLocationName((int)location);
        }

        public static string GetLocationName(int locationId)
        {
            string locationName = string.Empty;

            switch (locationId)
            {
                case 1:
                    locationName = "Location 1";
                    break;
                case 2:
                    locationName = "Location 2";
                    break;
            }

            return locationName;
        }

        public static Dictionary<InventoryChangeType, string> InventoryChangeTypes =>
           new Dictionary<InventoryChangeType, string>
           {
                {InventoryChangeType.Created, InventoryChangeType.Created.ToString() },
                 {InventoryChangeType.Decreased, InventoryChangeType.Decreased.ToString() },
                {InventoryChangeType.Increased, InventoryChangeType.Increased.ToString() },
               {InventoryChangeType.Transferred, InventoryChangeType.Transferred.ToString() }
           };
    }
}
