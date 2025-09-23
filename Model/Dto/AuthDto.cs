namespace LoginAPI.Model.Dto
{
    public class AuthDto
    {
        public record RegisterReq(string UserName, string Password, string ConfirmPassword);
        public record LoginReq(string UserName, string Password);
        public record AuthRes(string Token, string UserName);
    }
}
