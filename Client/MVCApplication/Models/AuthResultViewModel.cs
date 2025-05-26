namespace MVCApplication.Models
{
    public class AuthResultViewModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserInfoViewModel User { get; set; }
    }

    public class UserInfoViewModel

    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
