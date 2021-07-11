using MapperSample.Entities;
using NextGenMapper;

namespace MapperSample.Models
{
    [MapTo(typeof(UserEntity))]
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
    }
}
