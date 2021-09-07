using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.CommonModel
{
    public class UserInformation
    {
        public string IdentityId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Concat(this.FirstName, " ", this.LastName);
    }
}
