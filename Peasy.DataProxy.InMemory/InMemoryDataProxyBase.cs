using AutoMapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Peasy;
using Peasy.Extensions;

namespace Peasy.DataProxy.InMemory
{
    public abstract class InMemoryDataProxyBase<DTO, TKey> : IServiceDataProxy<DTO, TKey> where DTO : IDomainObject<TKey>
    {
        private object _lockObject = new object();

        protected InMemoryDataProxyBase()
        {
            Mapper.Initialize(config => config.CreateMap<DTO, DTO>());
            LoadData();
        }

        protected ConcurrentDictionary<TKey, DTO> Data { get; private set; }

        protected void LoadData()
        {
            Data = new ConcurrentDictionary<TKey, DTO>();
            SeedDataProxy().ForEach(item =>
            {
                var rule = item.ID.CreateValueRequiredRule("ID").Validate();
                if (!rule.IsValid || item.ID == null)
                    throw new ArgumentException($"All values for {item.GetType().Name}.ID must be supplied");
                if (Data.ContainsKey(item.ID))
                    throw new ArgumentException($"Duplicate ids are not allowed: id {item.ID}");
                Data[item.ID] = item;
            });
        }

        protected virtual IEnumerable<DTO> SeedDataProxy()
        {
            return Enumerable.Empty<DTO>();
        }

        protected abstract TKey GetNextID();

        public virtual IVersionContainer IncrementVersion(IVersionContainer versionContainer)
        {
            return versionContainer;
        }

        public virtual IEnumerable<DTO> GetAll()
        {
            Debug.WriteLine($"Executing {this.GetType().Name}.GetAll");
            return Data.Values.Select(Mapper.Map<DTO, DTO>).ToArray();
        }

        public virtual DTO GetByID(TKey id)
        {
            Debug.WriteLine($"Executing {this.GetType().Name}.GetByID");
            var item = Data[id];
            return Mapper.Map(item, default(DTO));
        }

        public virtual DTO Insert(DTO entity)
        {
            lock (_lockObject)
            {
                Debug.WriteLine($"Executing {this.GetType().Name}.Insert");
                var nextID = GetNextID();
                if (Data.ContainsKey(nextID))
                    throw new ArgumentException($"Duplicate ids are not allowed: id {nextID}");
                entity.ID = nextID;
                Data[nextID] = Mapper.Map(entity, default(DTO));
                return entity;
            }
        }

        public virtual DTO Update(DTO entity)
        {
            lock (_lockObject)
            {
                Debug.WriteLine($"Executing {this.GetType().Name}.Update");
                var existing = Data[entity.ID];
                if (entity is IVersionContainer)
                {
                    if ((entity as IVersionContainer).Version != (existing as IVersionContainer).Version)
                        throw new InvalidOperationException($"Cannot find a matching version for supplied {entity.GetType().Name}");
                    IncrementVersion(entity as IVersionContainer);
                }
                Mapper.Map(entity, existing);
                return entity;
            }
        }

        public virtual void Delete(TKey id)
        {
            Debug.WriteLine($"Executing {this.GetType().Name}.Delete");
            var entity = default(DTO);
            Data.TryRemove(id, out entity);
        }

        public virtual async Task<IEnumerable<DTO>> GetAllAsync()
        {
            return GetAll();
        }

        public virtual async Task<DTO> GetByIDAsync(TKey id)
        {
            return GetByID(id);
        }

        public virtual async Task<DTO> InsertAsync(DTO entity)
        {
            return Insert(entity);
        }

        public virtual async Task<DTO> UpdateAsync(DTO entity)
        {
            return Update(entity);
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            Delete(id);
        }

        public virtual bool SupportsTransactions
        {
            get { return false; }
        }

        public virtual bool IsLatencyProne
        {
            get { return false; }
        }

        public void Clear()
        {
            Data.Clear();
        }

    }
}