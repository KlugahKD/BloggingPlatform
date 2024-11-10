namespace BloggingPlatform.Business.Common;

public static class Constants
{
    public static class Response
    {
        public const string Success = "Success";
        public const string Error = "An Error Occurred";
        public const string InternalServerError = "Something Bad Happened. Please try again later.";
        public const string NotFound = "Sorry! Not Found";
    }
    
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
    
    public static class CreatedBy
    {
        public const string System = "System";
    }
}