using System;

namespace MusicSchoolApp
{
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static string Role { get; set; }
        public static DateTime LoginTime { get; set; }

        public static bool IsLoggedIn => UserId > 0;

        public static bool IsAdmin => Role?.ToLower() == "admin";

        public static bool IsStaff => Role?.ToLower() == "staff";


        public static bool CanManageUsers => IsAdmin;
        public static bool CanViewTeachers => IsAdmin || IsStaff; 
        public static bool CanAddTeachers => IsAdmin;
        public static bool CanEditTeachers => IsAdmin;
        public static bool CanDeleteTeachers => IsAdmin; 
        public static bool CanViewStudents => IsAdmin || IsStaff; 
        public static bool CanAddStudents => IsAdmin || IsStaff;
        public static bool CanEditStudents => IsAdmin || IsStaff;
        public static bool CanDeleteStudents => IsAdmin;

        public static bool CanViewInstruments => IsAdmin || IsStaff;
        public static bool CanAddInstruments => IsAdmin;
        public static bool CanEditInstruments => IsAdmin;
        public static bool CanDeleteInstruments => IsAdmin;
        public static bool CanManageInstrumentAssignments => IsAdmin || IsStaff;
        public static bool CanManageRepairs => IsAdmin || IsStaff; 
        public static bool CanViewSubjects => IsAdmin || IsStaff; 
        public static bool CanAddSubjects => IsAdmin; 
        public static bool CanEditSubjects => IsAdmin; 
        public static bool CanDeleteSubjects => IsAdmin;
        public static bool CanAssignStudentsToSubjects => IsAdmin || IsStaff; 
        public static bool CanViewRooms => IsAdmin || IsStaff; 
        public static bool CanAddRooms => IsAdmin; 
        public static bool CanEditRooms => IsAdmin || IsStaff; 
        public static bool CanDeleteRooms => IsAdmin; 
        public static bool CanViewSuppliers => IsAdmin || IsStaff;
        public static bool CanAddSuppliers => IsAdmin; 
        public static bool CanEditSuppliers => IsAdmin || IsStaff; 
        public static bool CanDeleteSuppliers => IsAdmin;
        public static bool CanManageSchedule => IsAdmin || IsStaff;
        public static bool CanViewReports => IsAdmin || IsStaff; 
        public static void Login(int userId, string username, string fullName, string role)
        {
            UserId = userId;
            Username = username;
            FullName = fullName;
            Role = role;
            LoginTime = DateTime.Now;
        }

        public static void Logout()
        {
            UserId = 0;
            Username = null;
            FullName = null;
            Role = null;
        }
    }
}