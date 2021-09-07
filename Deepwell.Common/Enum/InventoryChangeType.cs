using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.Enum
{
    public enum InventoryChangeType
    {
        NotSet = 0,
        Created = 1,
        Increased = 2,
        Decreased = 3,
        Transferred = 4,
        Ordered = 5,
        Returned = 6,
        Cancelled = 7,
        AllocatedToMud = 8
    }
}
