namespace NextGenMapper
{
    public class SampleMapper
    {
        public static User2 Map(User1 source) => new User2 { Name = source.Name, Email = source.Email, };
    }

    public class User1
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class User2
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
