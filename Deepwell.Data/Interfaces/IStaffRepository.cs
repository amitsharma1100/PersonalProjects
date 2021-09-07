using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Models;

namespace Deepwell.Data.Interfaces
{
    interface IStaffRepository
    {
        void AddStaffUser(Staff s);

        UserInformation GetUserInfoByIndentiyAndUserType(string identityId, TypeOfUser userType);
        IEnumerable<Staff> Search(UserSearchRequest s);
        Staff GetUserByEmployeeId(int employeeId);
        IEnumerable<string> Edit(Staff s, string identityId);
    }
}
