using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.DataAccess.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(MongoDbContext context)
        {
            _context = context;
            _collection = GetCollection();
        }

        private IMongoCollection<T> GetCollection()
        {
            var typeName = typeof(T).Name;
            return typeName switch
            {
                nameof(Preference) => (IMongoCollection<T>)(object)_context.Preferences,
                nameof(Customer) => (IMongoCollection<T>)(object)_context.Customers,
                nameof(PromoCode) => (IMongoCollection<T>)(object)_context.PromoCodes,
                _ => throw new NotSupportedException($"Collection for type {typeof(T).Name} is not configured.")
            };
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var entities = await _collection.Find(_ => true).ToListAsync();
            await PopulateNavigationPropertiesAsync(entities);
            return entities;
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (entity != null)
            {
                await PopulateNavigationPropertiesAsync(new List<T> { entity });
            }
            return entity;
        }

        public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids)
        {
            var entities = await _collection.Find(x => ids.Contains(x.Id)).ToListAsync();
            await PopulateNavigationPropertiesAsync(entities);
            return entities;
        }

        public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate)
        {
            var entity = await _collection.Find(predicate).FirstOrDefaultAsync();
            if (entity != null)
            {
                await PopulateNavigationPropertiesAsync(new List<T> { entity });
            }
            return entity;
        }

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
        {
            var entities = await _collection.Find(predicate).ToListAsync();
            await PopulateNavigationPropertiesAsync(entities);
            return entities;
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        }

        public async Task DeleteAsync(T entity)
        {
            await _collection.DeleteOneAsync(x => x.Id == entity.Id);
        }

        private async Task PopulateNavigationPropertiesAsync(List<T> entities)
        {
            if (typeof(T) == typeof(Customer))
            {
                await PopulateCustomerNavigationPropertiesAsync(entities.Cast<Customer>().ToList());
            }
            else if (typeof(T) == typeof(PromoCode))
            {
                await PopulatePromoCodeNavigationPropertiesAsync(entities.Cast<PromoCode>().ToList());
            }
        }

        private async Task PopulateCustomerNavigationPropertiesAsync(List<Customer> customers)
        {
            if (!customers.Any()) return;

            var allPreferences = await _context.Preferences.Find(_ => true).ToListAsync();
            var allPromoCodes = await _context.PromoCodes.Find(_ => true).ToListAsync();

            foreach (var customer in customers)
            {
                if (customer.Preferences != null)
                {
                    foreach (var cp in customer.Preferences)
                    {
                        cp.Customer = customer;
                        if (cp.Preference == null)
                        {
                            cp.Preference = allPreferences.FirstOrDefault(p => p.Id == cp.PreferenceId);
                        }
                    }
                }

                if (customer.PromoCodes != null)
                {
                    foreach (var pc in customer.PromoCodes)
                    {
                        pc.Customer = customer;
                        if (pc.PromoCode == null)
                        {
                            pc.PromoCode = allPromoCodes.FirstOrDefault(p => p.Id == pc.PromoCodeId);
                        }
                    }
                }
            }
        }

        private async Task PopulatePromoCodeNavigationPropertiesAsync(List<PromoCode> promoCodes)
        {
            if (!promoCodes.Any()) return;

            var allCustomers = await _context.Customers.Find(_ => true).ToListAsync();
            var allPreferences = await _context.Preferences.Find(_ => true).ToListAsync();

            foreach (var promoCode in promoCodes)
            {
                if (promoCode.Preference == null)
                {
                    promoCode.Preference = allPreferences.FirstOrDefault(p => p.Id == promoCode.PreferenceId);
                }

                if (promoCode.Customers != null)
                {
                    foreach (var pc in promoCode.Customers)
                    {
                        pc.PromoCode = promoCode;
                        if (pc.Customer == null)
                        {
                            pc.Customer = allCustomers.FirstOrDefault(c => c.Id == pc.CustomerId);
                        }
                    }
                }
            }
        }
    }
}