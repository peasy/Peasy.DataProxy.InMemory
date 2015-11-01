using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Peasy.Core;
using Shouldly;
using Moq;

namespace Peasy.DataProxy.InMemory.Tests
{
    [TestClass]
    public class InMemoryDataProxyBaseTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void throws_exception_if_seed_data_contains_duplicate_ids()
        {
            var dataProxy = new PersonDataProxyWithSeedDataContainingDuplicateIDs();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void throws_exception_if_seed_data_does_not_contain_ids()
        {
            var dataProxy = new PersonDataProxyWithSeedDataWithoutIDs();
        }

        [TestMethod]
        public void should_contain_seeded_data_on_initialize()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.GetAll().Count().ShouldBe(3);
        }

        [TestMethod]
        public void should_return_expected_item_on_GetByID()
        {
            var dataProxy = new PersonDataProxy();
            var person = dataProxy.GetByID(2);
            person.Name.ShouldBe("James Page");
        }

        [TestMethod]
        public void should_insert_item_into_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Brian May" };
            dataProxy.Insert(person);
            dataProxy.GetAll().Count().ShouldBe(4);
        }

        [TestMethod]
        public void should_insert_item_into_data_store_with_expected_id()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Brian May" };
            var newPerson = dataProxy.Insert(person);
            newPerson.ID.ShouldBe(4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void throws_exception_on_insert_if_id_retrieved_from_GetNextID_already_exists()
        {
            var dataProxy = new PersonDataProxyWithSeedDataWithoutIDs();
        }

        [TestMethod]
        public void should_update_item_in_data_store()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.GetAll().Count().ShouldBe(3);
        }
        
        [TestMethod]
        public void should_delete_item_in_data_store()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.GetAll().Count().ShouldBe(3);
        }

        [TestMethod]
        public void multiple_inserts_should_be_thread_safe()
        {

        }
        
        [TestMethod]
        public void multiple_updates_should_be_thread_safe()
        {

        }

        [TestMethod]
        public void multiple_deletes_should_be_thread_safe()
        {

        }

        [TestMethod]
        public void version_containers_should_be_updated()
        {

        }
    }

    public class Person : IDomainObject<int>
    {
        public int ID { get; set; }
        public string Name { get; set; }
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

    public class PersonDataProxy : TestDataProxyBase<Person>
    {
        protected override IEnumerable<Person> SeedDataProxy()
        {
            yield return new Person { ID = 1, Name = "Django Reinhardt" };
            yield return new Person { ID = 2, Name = "James Page" };
            yield return new Person { ID = 3, Name = "Eric Johnson" };
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
        public override IVersionContainer IncrementVersion(IVersionContainer versionContainer)
        {
            versionContainer.Version = (Convert.ToInt32(versionContainer.Version) + 1).ToString();
            return versionContainer;
        }
    }

    public class TestDataProxyBase<T> : InMemoryProxyBase<T, int> where T : IDomainObject<int>
    {
        protected override int GetNextID()
        {
            if (Data.Values.Any())
                return Data.Values.Max(c => c.ID) + 1;

            return 1;
        }
    }

}
