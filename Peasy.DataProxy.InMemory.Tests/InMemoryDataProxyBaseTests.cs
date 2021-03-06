﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Moq;
using System.Threading.Tasks;

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
            dataProxy.Delete(1);
            dataProxy.GetAll().Count().ShouldBe(2);
        }

        [TestMethod]
        public void multiple_inserts_should_be_thread_safe()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), (index) =>
            {
                dataProxy.Insert(new Person() { Name = $"Jim Morrison{index}" });
            });
            dataProxy.GetAll().Count().ShouldBe(count);
            foreach (var i in Enumerable.Range(0, count))
            {
                dataProxy.GetByID(i + 1).ShouldNotBe(null);
            }
        }
        
        [TestMethod]
        public void multiple_updates_should_be_thread_safe()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), (index) =>
            {
                dataProxy.Insert(new Person() { Name = $"Jim Morrison{index}" });
            });
            Parallel.ForEach(Enumerable.Range(0, count), (index) =>
            {
                var person = dataProxy.GetByID(index + 1);
                person.Name = $"Peter Frampton{index}";
                dataProxy.Update(person);
            });
            foreach (var i in Enumerable.Range(0, count))
            {
                dataProxy.GetByID(i + 1).Name.ShouldBe($"Peter Frampton{i}");
            }
        }

        [TestMethod]
        public void multiple_deletes_should_be_thread_safe()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), (index) =>
            {
                dataProxy.Insert(new Person() { Name = $"Jim Morrison{index}" });
            });
            Parallel.ForEach(Enumerable.Range(0, count), (index) =>
            {
                dataProxy.Delete(index + 1);
            });
            dataProxy.GetAll().Count().ShouldBe(0);
        }

        [TestMethod]
        public void version_containers_should_be_updated()
        {
            var dataProxy = new AddressDataProxy();
            var address = dataProxy.GetByID(1);
            address.Street = "234 Washington St";
            address = dataProxy.Update(address); 
            address.Version.ShouldBe("2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void update_throws_an_exception_when_version_container_version_differs_from_data_store_version()
        {
            var dataProxy = new AddressDataProxy();
            var address = dataProxy.GetByID(1);
            address.Version = "5";
            address = dataProxy.Update(address); 
        }
        
        [TestMethod]
        public void clear_removes_all_data()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            dataProxy.GetAll().Count().ShouldBe(0);
        }

        [TestMethod]
        public void SupportTransactions_should_return_false()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.SupportsTransactions.ShouldBe(false);
        }

        [TestMethod]
        public void IsLatencyProne_should_return_false()
        {
            var dataProxy = new PersonDataProxy();
            dataProxy.IsLatencyProne.ShouldBe(false);
        }
        
        [TestMethod]
        public async Task changing_returned_object_from_GetAll_does_not_change_state_in_the_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var people = await dataProxy.GetAllAsync();
            var person = people.First(p => p.ID == 1);
            person.Name = "FOO";
            people = await dataProxy.GetAllAsync();
            people.First(p => p.ID == 1).Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        public async Task should_return_expected_item_on_GetByID_async()
        {
            var dataProxy = new PersonDataProxy();
            var person = await dataProxy.GetByIDAsync(2);
            person.Name.ShouldBe("James Page");
        }

        [TestMethod]
        public async Task changing_returned_object_from_GetByID_does_not_change_state_in_the_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var person = await dataProxy.GetByIDAsync(1);
            person.Name = "FOO";
            var personInDataStore = await dataProxy.GetByIDAsync(person.ID);
            personInDataStore.Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        public async Task should_insert_item_into_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Brian May" };
            await dataProxy.InsertAsync(person);
            var people = await dataProxy.GetAllAsync();
            people.Count().ShouldBe(4);
        }

        [TestMethod]
        public async Task should_insert_item_into_data_store_with_expected_id_async()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Frank Zappa" };
            var newPerson = await dataProxy.InsertAsync(person);
            newPerson.ID.ShouldBe(4);
        }

        [TestMethod]
        public async Task changing_returned_object_from_insert_does_not_change_state_in_the_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var person = new Person() { Name = "Frank Zappa" };
            var newPerson = await dataProxy.InsertAsync(person);
            newPerson.Name = "FOO";
            var personInDataStore = await dataProxy.GetByIDAsync(newPerson.ID);
            personInDataStore.Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task throws_exception_on_insert_if_id_retrieved_from_GetNextID_already_exists_async()
        {
            var dataProxy = new PersonDataProxyWithBadIDIncrementLogic();
            await dataProxy.InsertAsync(new Person { Name = "Steve How" });
        }

        [TestMethod]
        public async Task should_update_item_in_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var newName = "Robby Krieger";
            var person = await dataProxy.GetByIDAsync(1);
            person.Name = newName;
            await dataProxy.UpdateAsync(person);
            var personInDataStore = await dataProxy.GetByIDAsync(1);
            personInDataStore.Name.ShouldBe(newName);
        }
        
        [TestMethod]
        public async Task changing_returned_object_from_Update_does_not_change_state_in_the_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            var newName = "Robby Krieger";
            var person = await dataProxy.GetByIDAsync(1);
            person.Name = newName;
            var updatedPerson = await dataProxy.UpdateAsync(person);
            updatedPerson.Name = "FOO";
            var personInDataStore = await dataProxy.GetByIDAsync(1);
            personInDataStore.Name.ShouldNotBe("FOO");
        }

        [TestMethod]
        public async Task should_delete_item_in_data_store_async()
        {
            var dataProxy = new PersonDataProxy();
            await dataProxy.DeleteAsync(1);
            var people = await dataProxy.GetAllAsync();
            people.Count().ShouldBe(2);
        }

        [TestMethod]
        public async Task multiple_inserts_should_be_thread_safe_async()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), async (index) =>
            {
                await dataProxy.InsertAsync(new Person() { Name = $"Jim Morrison{index}" });
            });
            dataProxy.GetAll().Count().ShouldBe(count);
            foreach (var i in Enumerable.Range(0, count))
            {
                var person = await dataProxy.GetByIDAsync(i + 1);
                person.ShouldNotBe(null);
            }
        }

        [TestMethod]
        public async Task multiple_updates_should_be_thread_safe_async()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), async (index) =>
            {
                await dataProxy.InsertAsync(new Person() { Name = $"Jim Morrison{index}" });
            });
            Parallel.ForEach(Enumerable.Range(0, count), async (index) =>
            {
                var person = await dataProxy.GetByIDAsync(index + 1);
                person.Name = $"Peter Frampton{index}";
                await dataProxy.UpdateAsync(person);
            });
            foreach (var i in Enumerable.Range(0, count))
            {
                var person = await dataProxy.GetByIDAsync(i + 1);
                person.Name.ShouldBe($"Peter Frampton{i}");
            }
        }

        [TestMethod]
        public async Task multiple_deletes_should_be_thread_safe_async()
        {
            var count = 50;
            var dataProxy = new PersonDataProxy();
            dataProxy.Clear();
            Parallel.ForEach(Enumerable.Range(0, count), async (index) =>
            {
                await dataProxy.InsertAsync(new Person() { Name = $"Jim Morrison{index}" });
            });
            Parallel.ForEach(Enumerable.Range(0, count), async (index) =>
            {
                await dataProxy.DeleteAsync(index + 1);
            });
            var people = await dataProxy.GetAllAsync();
            people.Count().ShouldBe(0);
        }

        [TestMethod]
        public async Task version_containers_should_be_updated_async()
        {
            var dataProxy = new AddressDataProxy();
            var address = await dataProxy.GetByIDAsync(1);
            address.Street = "234 Washington St";
            address = await dataProxy.UpdateAsync(address); 
            address.Version.ShouldBe("2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task update_throws_an_exception_when_version_container_version_differs_from_data_store_version_async()
        {
            var dataProxy = new AddressDataProxy();
            var address = await dataProxy.GetByIDAsync(1);
            address.Version = "5";
            address = await dataProxy.UpdateAsync(address); 
        }
    }
}
