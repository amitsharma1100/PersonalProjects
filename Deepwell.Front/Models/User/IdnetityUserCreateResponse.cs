using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Deepwell.Front.Models.User
{
    public class IdnetityUserCreateResponse
    {
        public bool IsSucceeded { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public string IdentityId { get; set; }
    }
}