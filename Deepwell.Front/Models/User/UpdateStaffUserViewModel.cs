using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.User
{
    public class UpdateStaffUserViewModel : StaffUserViewModel
    {
        public string IdentityId { get; set; }
        public string CurrentRole { get; set; }
    }
}