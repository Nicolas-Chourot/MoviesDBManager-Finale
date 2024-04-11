using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace MoviesDBManager.Models
{
    public static class OnlineUsers
    {
        public static List<int> ConnectedUsersId
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsers"] == null)
                    HttpRuntime.Cache["OnLineUsers"] = new List<int>();
                return (List<int>)HttpRuntime.Cache["OnLineUsers"];
            }
        }
        private static string SerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsersSerialNumber"] == null)
                    SetHasChanged();
                return (string)HttpRuntime.Cache["OnLineUsersSerialNumber"];
            }
            set
            {
                HttpRuntime.Cache["OnLineUsersSerialNumber"] = value;
            }
        }
        public static bool HasChanged()
        {
            if (HttpContext.Current.Session["SerialNumber"] == null)
            {
                HttpContext.Current.Session["SerialNumber"] = SerialNumber;
                return true;
            }
            string sessionSerialNumber = (string)HttpContext.Current.Session["SerialNumber"];
            HttpContext.Current.Session["SerialNumber"] = SerialNumber;
            return SerialNumber != sessionSerialNumber;
        }
        public static void SetHasChanged()
        {
            SerialNumber = Guid.NewGuid().ToString();
        }
        public static void AddSessionUser(int userId)
        {
            HttpContext.Current.Session["UserId"] = userId;
            ConnectedUsersId.Add(userId);
            SetHasChanged();
        }
        public static void RemoveSessionUser()
        {
            User user = GetSessionUser();
            if (user != null)
             ConnectedUsersId.Remove(user.Id);
            HttpContext.Current?.Session.Abandon();
            SetHasChanged();
        }
        public static bool IsOnLine(int userId)
        {
            return ConnectedUsersId.Contains(userId);
        }
        public static User GetSessionUser()
        {
            if (HttpContext.Current.Session["UserId"] != null)
            {
                User currentUser = DB.Users.Get((int)HttpContext.Current.Session["UserId"]);
                return currentUser;
            }
            return null;
        }
        public static bool Write_Access()
        {
            User sessionUser = OnlineUsers.GetSessionUser();
            if (sessionUser != null)
            {
                return sessionUser.IsPowerUser || sessionUser.IsAdmin;
            }
            return false;
        }

        public class UserAccess : AuthorizeAttribute
        {
            private bool ServerSideResponseHandling { get; set; }
            public UserAccess(bool serverSideResponseHandling = true)
            {
                ServerSideResponseHandling = serverSideResponseHandling;
            }
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null)
                {
                    if (sessionUser.Blocked)
                    {
                        OnlineUsers.RemoveSessionUser();
                        if (ServerSideResponseHandling)
                        {
                            httpContext.Response.Redirect("~/Accounts/Login?message=Compte bloqué!");
                            return false;
                        }
                        else
                        {
                            httpContext.Response.StatusCode = 403; // Forbiden status
                        }
                    }
                    return true;
                }
                httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                return false;

            }
        }
        public class PowerUserAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && (sessionUser.IsPowerUser || sessionUser.IsAdmin))
                    return true;
                else
                {
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!", true);
                }
                return false;
            }
        }
        public class AdminAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && sessionUser.IsAdmin)
                    return true;
                else
                {
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                }
                return true;
            }
        }
    }
}