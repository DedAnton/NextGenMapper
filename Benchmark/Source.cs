namespace Benchmark
{
    public static class Source
    {
        public static string GetSourceCode(int number) => $@"

using System;
using System.Collections.Generic;
using NextGenMapper;

namespace Benchmark{number}
{{
    [Mapper]
    public class SourceCode{number}
    {{
        public void Invocation()
        {{
            var source1 = new Source1 {{ Name = ""Anton"", Birthday = new DateTime(1997, 05, 20), ToInitializer = ""Hello"" }};
            var destination1 = source1.Map<Destination1>();

            var source2 = new Source2 {{ FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) }};
            var destination2 = source2.Map<Destination2>();

            var source3 = new Source3 {{ FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo"", Birthday = new DateTime(1997, 05, 20) }};
            var destination3 = source3.Map<Destination3>();

            var source4 = new Source4(""Anton"", new DateTime(1997, 05, 20));
            var destination4 = source4.Map<Destination4>();

            var source5 = EnumFrom.ValueB;
            var destination5 = source5.Map<EnumTo>();

            var source6_1 = new Source6 {{ Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) }};
            var source6_2 = new Source6 {{ Name = ""Roman"", Birthday = new DateTime(1996, 07, 08) }};
            var source6 = new List<Source6> {{ source6_1, source6_2 }};
            var destination6 = source6.Map<List<Destination6>>();

            var source7 = new User7 {{ Address = new Address7 {{ City = ""Dinamo"", Street = ""3rd Builders Street"" }} }};
            var destination7 = source7.Map<UserFlat7>();

            var source8 = new UserFlat8 {{ AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"" }};
            var destination8 = source8.Map<User8>();

        }}
        
        public Destination2 Map(Source2 source) => new Destination2 {{ Name = $""{{ source.FirstName }} {{ source.SecondName }}"", Birthday = source.Birthday.ToShortDateString() }};
        [Partial]
        public Destination9 Map(Source9 source) => new Destination9 {{ Name = $""{{ source.FirstName }} {{ source.SecondName }}"", Birthday = source.Birthday.ToShortDateString() }};
        [Partial]
        public Destination3 Map(Source3 source) => new Destination3($""{{source.FirstName}} {{source.SecondName}}"", default) {{ }};
    }}

    public class Source1
        {{
            public string Name {{ get; set; }}
            public DateTime Birthday {{ get; set; }}
            public string ToInitializer {{ get; set; }}
        }}

        public class Destination1
        {{
            public Destination1(string name, DateTime birthday)
            {{
                Name = name;
                Birthday = birthday;
            }}

            public string Name {{ get; }}
            public DateTime Birthday {{ get; }}
            public string ToInitializer {{ get; set; }}
        }}

        public class Source2
        {{
            public string FirstName {{ get; set; }}
            public string SecondName {{ get; set; }}
            public DateTime Birthday {{ get; set; }}
        }}

        public class Destination2
        {{
            public string Name {{ get; set; }}
            public string Birthday {{ get; set; }}
        }}

        public class Source3
        {{
            public string FirstName {{ get; set; }}
            public string SecondName {{ get; set; }}
            public int Height {{ get; set; }}
            public string City {{ get; set; }}
            public DateTime Birthday {{ get; set; }}
        }}

        public class Destination3
        {{
            public string Name {{ get; }}
            public int Height {{ get; }}
            public string City {{ get; set; }}
            public DateTime Birthday {{ get; set; }}

            public Destination3() {{ }}

            public Destination3(string name, int height)
            {{
                Name = name;
                Height = height;
            }}
        }}

        public record Source4(string Name, DateTime Birthday);
        public record Destination4(string Name, DateTime Birthday);

        public enum EnumFrom
        {{
            ValueA,
            ValueB,
            ValueC
        }}

        public enum EnumTo
        {{
            valueA = 10,
            valueB = 20,
            valueC = 30
        }}

        public class Source6
        {{
            public string Name {{ get; set; }}
            public DateTime Birthday {{ get; set; }}
            public string ToInitializer {{ get; set; }}
        }}

        public class Destination6
        {{
            public Destination6(string name, DateTime birthday)
            {{
                Name = name;
                Birthday = birthday;
            }}

            public string Name {{ get; }}
            public DateTime Birthday {{ get; }}
            public string ToInitializer {{ get; set; }}
        }}

        public class User7
        {{
            public Address7 Address {{ get; set; }}
        }}

        public class Address7
        {{
            public string City {{ get; set; }}
            public string Street {{ get; set; }}
        }}

        public class UserFlat7
        {{
            public string AddressCity {{ get; set; }}
            public string AddressStreet {{ get; set; }}
        }}

        public class User8
        {{
            public Address8 Address {{ get; set; }}
        }}

        public class Address8
        {{
            public string City {{ get; set; }}
            public string Street {{ get; set; }}
        }}

        public class UserFlat8
        {{
            public string AddressCity {{ get; set; }}
            public string AddressStreet {{ get; set; }}
        }}

        public class Source9
        {{
            public string FirstName {{ get; set; }}
            public string SecondName {{ get; set; }}
            public DateTime Birthday {{ get; set; }}
            public int Height {{ get; set; }}
        }}

        public class Destination9
        {{
            public string Name {{ get; set; }}
            public string Birthday {{ get; set; }}
            public int Height {{ get; set; }}
        }}
}}

";
    }
}
