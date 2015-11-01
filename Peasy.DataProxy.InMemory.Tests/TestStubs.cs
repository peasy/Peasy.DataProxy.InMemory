using Peasy.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peasy.DataProxy.InMemory.Tests
{
    public class Person : IDomainObject<int>
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class PersonDataProxy : TestDataProxyBase<Person>
    {
        protected override IEnumerable<Person> SeedDataProxy()
        {
            yield return new Person { ID = 1, Name = "Django Reinhardt" };
            yield return new Person { ID = 2, Name = "James Page" };
            yield return new Person { ID = 3, Name = "Eric Johnson" };
        }
    }

    public class PersonDataProxyWithSeedDataContainingDuplicateIDs : TestDataProxyBase<Person> 
    {
        protected override IEnumerable<Person> SeedDataProxy()
        {
            yield return new Person { ID = 1, Name = "Django Reinhardt" };
            yield return new Person { ID = 1, Name = "James Page" };
        }
    }

    public class PersonDataProxyWithSeedDataWithoutIDs : TestDataProxyBase<Person> 
    {
        protected override IEnumerable<Person> SeedDataProxy()
        {
            yield return new Person { Name = "Django Reinhardt" };
            yield return new Person { Name = "James Page" };
        }
    }

    public class PersonDataProxyWithBadIDIncrementLogic : InMemoryDataProxyBase<Person, int>
    {
        protected override IEnumerable<Person> SeedDataProxy()
        {
            yield return new Person { ID = 1, Name = "James Page" };
        }

        protected override int GetNextID()
        {
            return 1;
        }
    }

    public class Address : IDomainObject<int>, IVersionContainer
    {
        public int ID { get; set; }
        public string Street { get; set; }
        public string Version { get; set; }
    }

    public class AddressDataProxy : TestDataProxyBase<Address>
    {
        protected override IEnumerable<Address> SeedDataProxy()
        {
            yield return new Address { ID = 1, Street = "123 Main St.", Version = "1" };
        }
        
        public override IVersionContainer IncrementVersion(IVersionContainer versionContainer)
        {
            versionContainer.Version = (Convert.ToInt32(versionContainer.Version) + 1).ToString();
            return versionContainer;
        }
    }

    public class TestDataProxyBase<T> : InMemoryDataProxyBase<T, int> where T : IDomainObject<int>
    {
        protected override int GetNextID()
        {
            if (Data.Values.Any())
                return Data.Values.Max(c => c.ID) + 1;

            return 1;
        }
    }
}
