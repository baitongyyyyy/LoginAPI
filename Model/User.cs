namespace LoginAPI.Model
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow; 
    }
}
