using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using System;
using System.Web;
using System.Web.SessionState;

namespace Deepwell.Common.Helpers
{
    /// <summary>
    ///     Helper class for managing Session state.
    /// </summary>
    public class SessionHelper
    {
        private static HttpSessionState sessionState => HttpContext.Current.Session;
        private const string USER_ID_KEY = "UserId";
        private const string IDENTITY_ID_KEY = "IdentityId";
        private const string USER_TYPE_KEY = "UserType";
        private const string LOCATION_KEY = "Location";
        private const string FULLNAME_KEY = "FullName";
               
        /// <summary>
        ///     Indicates whether user is logged in.
        /// </summary>
        public static bool IsUserLoggedIn => IdentityId.Trim().HasValue();

        /// <summary>
        ///     Id of the logged in User.
        /// </summary>
        public static int UserId
        {
            get
            {
                return Convert.ToInt32(sessionState[USER_ID_KEY]);
            }
            set
            {
                sessionState[USER_ID_KEY] = value;
            }
        }

        /// <summary>
        ///     Identity Id of the logged in User.
        /// </summary>
        public static string IdentityId
        {
            get
            {
                return sessionState[IDENTITY_ID_KEY].IsNotNull() && sessionState[IDENTITY_ID_KEY].ToString().HasValue()
                    ? sessionState[IDENTITY_ID_KEY].ToString()
                    : string.Empty;
            }
            set
            {
                sessionState[IDENTITY_ID_KEY] = value;
            }
        }

        /// <summary>
        ///     User type. (Administrator, Staff, Customer or Vendor)
        /// </summary>
        public static TypeOfUser UserType
        {
            get
            {
                return IsUserLoggedIn
                    ? (TypeOfUser)sessionState[USER_TYPE_KEY]
                    : TypeOfUser.NotSet;
            }
            set
            {
                sessionState[USER_TYPE_KEY] = value;
            }
        }

        /// <summary>
        ///     User Location.
        /// </summary>
        public static InventoryLocation Location
        {
            get
            {
                var loc = sessionState[LOCATION_KEY];
                if(loc.IsNotNull())
                {
                    return (InventoryLocation)loc;
                }
                else
                {
                    return InventoryLocation.NotSet;
                }
                
            }
            set
            {
                sessionState[LOCATION_KEY] = value;
            }
        }

        public static int LocationId => (int)Location; 

        public static string LocationName
        {
            get
            {
                string locationName = string.Empty;

                switch (Location)
                {                 
                    case InventoryLocation.One:
                        locationName = "Location 1";
                        break;
                    case InventoryLocation.Two:
                        locationName = "Location 2";
                        break;
                
                }

                return locationName;
            }
        }

        public static string FullName {
            get
            {
                return sessionState[FULLNAME_KEY].ToString();
            }
            set
            {
                sessionState[FULLNAME_KEY] = value;
            }
        }

        public static void CreateSession(string identityId, int userId, string fullName, TypeOfUser typeOfUser, InventoryLocation location)
        {
            IdentityId = identityId;
            FullName = fullName;
            UserId = userId;            
            UserType = typeOfUser;
            Location = location;
        }

        /// <summary>
        ///     Clears the session.
        /// </summary>
        public static void ClearSession()
        {
            sessionState.RemoveAll();
            sessionState.Clear();
        }

        public static bool IsUserAnAdministrator()
        {
            return UserType.Equals(TypeOfUser.Administrator);
        }
    }
}
