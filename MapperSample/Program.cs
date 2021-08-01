using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NextGenMapper;

namespace MapperSample
{
    public class Program
    {
        static void Main(string[] args)
        {
            var source1 = new Source { Name = "Anton", Birthday = new DateTime(1997, 05, 20) };
            var source2 = new Source { Name = "Roman", Birthday = new DateTime(1996, 07, 08) };
            var source = new System.Collections.Generic.List<Source> { source1, source2 };


            var destination = source.Map<List<Destination>>();

            Console.WriteLine(destination.FirstOrDefault()?.Name);
            //ASD();
            //var destination = source.Map<List<Destination>>();
        }

        private static void ASD()
        {
            var source1 = new Source { Name = "Anton", Birthday = new DateTime(1997, 05, 20) };
            var source2 = new Source { Name = "Roman", Birthday = new DateTime(1996, 07, 08) };
            var source = new List<Source> { source1, source2 };

            //var destination = source.Map<List<Destination>>();

            var isValid = true;

            //if (!isValid) throw new MapFailedException(source, destination);
        }

        public Destination Map<To>(Source source) => new Destination() { Name = source.Name, Birthday = source.Birthday, };

        public List<Destination> Map<To>(List<Source> sources) => sources.Select(x => x.Map<Destination>()).ToList();

        //public static Destination Map<To>(this Source source) => new Destination() { Name = source.Name, Birthday = source.Birthday, };

        //public static List<Destination> Map<To>(this List<Source> source) 
        //    => new List<Destination>(source.Capacity) 
        //    { 
        //        //System.Collections.IList.Item = source.System.Collections.IList.Item,
        //    };

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
    }
}
