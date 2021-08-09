using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NextGenMapper;

namespace MapperSample
{
    public class Program
    {
        public class PhoneNumber
        {
            public string Code { get; set; }
            public string Number { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string Street { get; set; }
        }

        public class UserFlat
        {
            public string PhoneNumberNumber { get; set; }
            public string PhoneNumberCode { get; set; }
        }
        private static PhoneNumber UnflatteningMap_TestPhone(UserFlat source, PhoneNumber descriminator = null) => new PhoneNumber
        (

        )
        {
            Code = source.PhoneNumberCode,
            Number = source.PhoneNumberNumber,
        };

        private static Address UnflatteningMap(UserFlat source, Address descriminator = null) => new Address
        (

        )
        {
            City = source.PhoneNumberCode,
            Street = source.PhoneNumberNumber,
        };

        static void Main(string[] args)
        {
            var user = new UserFlat();
            var result = UnflatteningMap(user);
        }
    }
}
