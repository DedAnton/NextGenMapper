using System.Linq;

namespace NextGenMapper
{
    public static class SampleMapper
    {
        public static User2 Map<TTo>(this User1 source) => new User2 { Name = source.Name, Email = source.Email, };

        public static Complex2 Map<TTo>(this Complex1 source) => new Complex2 { Id = source.Id, User = source.User.Map<User2>() };

        public static User2 Map2<TTo>(this User1 source)
        {
            User2 UserFunction(User1 userFunctionSource)
            {
                var names = userFunctionSource.Name?.Split(' ');

                return new User2
                {
                    Name = $"Name: {names.FirstOrDefault()}"
                };
            }

            var result = UserFunction(source);
            result.Email = source.Email;

            return result;
        }
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

    public class Complex1
    {
        public int Id { get; set; }
        public User1 User { get; set; }
    }

    public class Complex2
    {
        public int Id { get; set; }
        public User2 User { get; set; }
    }
}
