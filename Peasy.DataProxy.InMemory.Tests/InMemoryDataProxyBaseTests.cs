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
        public void changing_returned_object_from_GetAll_does_not_change_state_in_the_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var person = dataProxy.GetAll().First(p => p.ID == 1);
            person.Name = "FOO";
            dataProxy.GetAll().First(p => p.ID == 1).Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        public void should_return_expected_item_on_GetByID()
        {
            var dataProxy = new PersonDataProxy();
            var person = dataProxy.GetByID(2);
            person.Name.ShouldBe("James Page");
        }

        [TestMethod]
        public void changing_returned_object_from_GetByID_does_not_change_state_in_the_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var person = dataProxy.GetByID(1);
            person.Name = "FOO";
            dataProxy.GetByID(person.ID).Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        public void should_insert_item_into_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Brian May" };
            var x = dataProxy.Insert(person);
            dataProxy.GetAll().Count().ShouldBe(4);
        }

        [TestMethod]
        public void should_insert_item_into_data_store_with_expected_id()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Frank Zappa" };
            var newPerson = dataProxy.Insert(person);
            newPerson.ID.ShouldBe(4);
        }

        [TestMethod]
        public void changing_returned_object_from_insert_does_not_change_state_in_the_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Frank Zappa" };
            var newPerson = dataProxy.Insert(person);
            newPerson.Name = "FOO";
            dataProxy.GetByID(newPerson.ID).Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void throws_exception_on_insert_if_id_retrieved_from_GetNextID_already_exists()
        {
            var dataProxy = new PersonDataProxyWithBadIDIncrementLogic();
            dataProxy.Insert(new Person { Name = "Steve How" });
        }

        [TestMethod]
        public void should_update_item_in_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var newName = "Robby Krieger";
            var person = dataProxy.GetByID(1);
            person.Name = newName;
            dataProxy.Update(person);
            var personInDataStore = dataProxy.GetByID(1);
            personInDataStore.Name.ShouldBe(newName);
        }
        
        [TestMethod]
        public void changing_returned_object_from_Update_does_not_change_state_in_the_data_store()
        {
            var dataProxy = new PersonDataProxy();
            var newName = "Robby Krieger";
            var person = dataProxy.GetByID(1);
            person.Name = newName;
            var updatedPerson = dataProxy.Update(person);
            updatedPerson.Name = "FOO";
            var personInDataStore = dataProxy.GetByID(1);
            personInDataStore.Name.ShouldNotBe("FOO");
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
}
