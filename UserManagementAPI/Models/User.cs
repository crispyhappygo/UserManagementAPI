namespace UserManagementAPI.Models
{
    public class User
    {
        public int Id{get;set;}
        public string Username{get;set;} = string.Empty;
        public int Age {get;set;}
        public string Department {get;set;} = string.Empty;
    }
}