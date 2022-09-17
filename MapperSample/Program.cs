using NextGenMapper;
using System;

namespace MapperSample
{
    public class Program
    {
        public class Source
        {
            public string Name { get; set; }
            public DateTime Birthday { get; set; }
        }
        public class Destination
        {
            public string Name { get; set; }
            public DateTime Birthday { get; set; }
        }

        public static void Main()
        {
            var source = new Source { Name = "Anton", Birthday = new DateTime(1997, 05, 20) };

            var destination = source.Map<Destination>();

            Console.WriteLine(destination);
        }
    }
}