using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
    public class EfRepository : IRepository
    {
        private readonly AppDbContext _dbContext;

		public EfRepository(AppDbContext dbContext) => _dbContext = dbContext;

        public T GetById<T>(Guid id) where T : BaseEntity
        {
            return _dbContext.Set<T>().SingleOrDefault(e => e.Id == id);
        }

        public List<T> List<T>() where T : BaseEntity
        {
            return _dbContext.Set<T>().ToList();
        }

        public T Add<T>(T entity) where T : BaseEntity
        {
            _dbContext.Set<T>().Add(entity);
            _dbContext.SaveChanges();

            return entity;
        }

        public void Delete<T>(T entity) where T : BaseEntity
        {
            _dbContext.Set<T>().Remove(entity);
            _dbContext.SaveChanges();
        }

        public void Update<T>(T entity) where T : BaseEntity
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public bool EntityExists<T>(Guid entity) where T : BaseEntity => 
            _dbContext.Set<T>().Any(t => t.Id == entity);

        public void UpdateEntity<T>(T entity) where T : BaseEntity
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public bool Commit() => _dbContext.SaveChanges() >= 0;
    }
}