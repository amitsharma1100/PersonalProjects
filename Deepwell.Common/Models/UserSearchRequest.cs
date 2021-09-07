using Deepwell.Common.Enum;

namespace Deepwell.Common.Models
{
    public class UserSearchRequest
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IsActiveOptions ActiveOption { get; set; }
    }
}
