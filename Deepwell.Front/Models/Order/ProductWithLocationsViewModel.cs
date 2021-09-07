using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Deepwell.Common.Enum;
using Deepwell.CommonModels;

using Deepwell.Common.Extensions;

namespace Deepwell.Front.Models.Order
{
    public class ProductWithLocationsViewModel : ProductWithLocations
    {
        public string TaxableLabel => this.IsTaxable
            ? "Yes"
            : "No";

        public string LocationLabel => this.LocationId == 1
            ? "Location 1"
            : "Location 2";

        public int QuantityAvailable { get; set; }
    }
}