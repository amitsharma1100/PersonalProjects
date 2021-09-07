using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Models;
using Deepwell.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public class StaffRepository : IStaffRepository
    {
        private DeepwellContext _deepwellContext;

        public StaffRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public StaffRepository(DeepwellContext deepwellContext)
        {
            _deepwellContext = deepwellContext;
        }

        public void AddStaffUser(Staff s)
        {
            var result = _deepwellContext.Staffs.Add(s);
            _deepwellContext.SaveChanges();
        }

        public UserInformation GetUserInfoByIndentiyAndUserType(string identityId, TypeOfUser userType)
        {
            var userInfo = new UserInformation();
            switch (userType)
            {
                case TypeOfUser.Administrator:
                case TypeOfUser.Staff:
                    userInfo = _deepwellContext.Staffs.Where(s => s.IdentityId == identityId)
                           .Select(s => new UserInformation { UserId = s.UserId, FirstName = s.FirstName, LastName = s.LastName, IdentityId = s.IdentityId })
                           .FirstOrDefault();
                    break;
                case TypeOfUser.Customer:
                    break;
                case TypeOfUser.Vendor:
                    break;
                default:
                    break;
            }

            return userInfo;
        }

        public IEnumerable<Staff> Search(UserSearchRequest staff)
        {
            var allUsers = _deepwellContext.Staffs;
            IEnumerable<Staff> response = allUsers;

            if (staff.EmployeeId.HasValue())
            {
                response = response.Where(s => s.UserId == Convert.ToInt32(staff.EmployeeId));
            }

            if (staff.FirstName.HasValue())
            {
                response = response.Where(s => s.FirstName.ToLower().Contains(staff.FirstName.ToLower()));
            }

            if (staff.LastName.HasValue())
            {
                response = response.Where(s => s.LastName.ToLower().Contains(staff.LastName.ToLower()));
            }

            IsActiveOptions activeOption = staff.ActiveOption;

            switch (activeOption)
            {
                case IsActiveOptions.All:
                default:
                    {
                        return response.ToList();
                    }

                case IsActiveOptions.Yes:
                    {
                        return response.Where(s => s.IsActive == true).ToList();
                    }

                case IsActiveOptions.No:
                    {
                        return response.Where(s => s.IsActive == false).ToList();
                    }
            }
        }

        public Staff GetUserByEmployeeId(int employeeId)
        {
            var staff = new Staff();
            var result = _deepwellContext.Staffs.Where(s => s.UserId.Equals(employeeId));
            if (result.IsNotNull())
            {
                return result.FirstOrDefault();
            }

            return staff;
        }

        public Staff GetUserByEmail(string email)
        {
            var staff = new Staff();
            var result = _deepwellContext.Staffs.Where(s => s.AspNetUser.Email.Equals(email));
            if (result.IsNotNull())
            {
                return result.FirstOrDefault();
            }

            return staff;
        }

        public Staff GetUserByIdentityId(string identityId)
        {

            var staff = new Staff();
            var result = _deepwellContext.Staffs.Where(s => s.IdentityId.Equals(identityId));
            if (result.IsNotNull())
            {
                return result.FirstOrDefault();
            }

            return staff;
        }

        public IEnumerable<string> Edit(Staff userToUpdate, string identityId)
        {
            Staff currentUser = this.GetUserByIdentityId(identityId);
            var messages = new List<string>();
            bool isUpdated = false;


            if (currentUser.AspNetUser.Email.IsNotEqualTo(userToUpdate.AspNetUser.Email))
            {
                messages.Add(this.UpdateEmployeeEmail(ref currentUser, userToUpdate.AspNetUser.Email));
                isUpdated = true;
            }

            if (currentUser.FirstName.IsNotEqualTo(userToUpdate.FirstName))
            {
                currentUser.FirstName = userToUpdate.FirstName;
                isUpdated = true;
            }
            if (currentUser.LastName.IsNotEqualTo(userToUpdate.LastName))
            {
                currentUser.LastName = userToUpdate.LastName;
                isUpdated = true;
            }
            if (currentUser.IsActive.Equals(userToUpdate.IsActive) == false)
            {
                currentUser.IsActive = userToUpdate.IsActive;
                isUpdated = true;
            }

            if (isUpdated)
            {
                currentUser.DateModified = DateTime.Now;
                _deepwellContext.SaveChanges();
            }

            return messages;
        }

        public bool Remove(string identityId)
        {
            bool isSuccess = true;
            Staff staffUser = _deepwellContext.Staffs.FirstOrDefault(s => s.IdentityId == identityId);
            if (staffUser != null)
            {
                _deepwellContext.Staffs.Remove(staffUser);
                isSuccess = _deepwellContext.SaveChanges() > 0;
            }

            return isSuccess;
        }

        private string UpdateEmployeeEmail(ref Staff currentUser, string newEmail)
        {
            try
            {
                var userByEmail = this.GetUserByEmail(newEmail);
                if (userByEmail.IsNotNull())
                {
                    return DisplayText.User.EmailAlreadyExistsErrorMessage;
                }
                else
                {
                    currentUser.AspNetUser.Email = newEmail;
                    currentUser.AspNetUser.UserName = newEmail;
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
