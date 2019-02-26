using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Infrastructure
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IPropertyMappingService _propertyMappingService;

        public Repository(AppDbContext dbContext, IPropertyMappingService propertyMappingService)
        {
            _dbContext = dbContext;
            _propertyMappingService = propertyMappingService;
        }

        public T GetEntity<T>(Guid id) where T : BaseEntity =>
            _dbContext.Set<T>().SingleOrDefault(e => e.Id == id);

        public List<TEntity> GetCollection<TEntity>(ISpecification<TEntity> specification = null)
            where TEntity : BaseEntity
        {
            if (specification == null) return _dbContext.Set<TEntity>().ToList();

            var queryableResultWithIncludes = specification.Includes
                .Aggregate(_dbContext.Set<TEntity>().AsQueryable(),
                    (current, include) => current.Include(include));

            var secondaryResult = specification.IncludeStrings
                .Aggregate(queryableResultWithIncludes,
                    (current, include) => current.Include(include));

            return secondaryResult
                .Where(specification.Criteria).ToList();
        }

        public PagedList<TEntity> GetCollection<TEntity, TDto>(IParameters parameters,
            ISpecification<TEntity> specification)
            where TEntity : BaseEntity where TDto : IDto
        {
            var entities = GetCollection(specification)
                .ApplySort(parameters.OrderBy,
                    _propertyMappingService.GetPropertyMapping<TDto, TEntity>()).ToList();

            return PagedList<TEntity>.Create(entities, parameters.PageNumber, parameters.PageSize);
        }

        public TEntity GetFirstOrDefault<TEntity>(ISpecification<TEntity> specification = null)
            where TEntity : BaseEntity =>
            specification == null
                ? _dbContext.Set<TEntity>().FirstOrDefault()
                : _dbContext.Set<TEntity>().FirstOrDefault(specification.Criteria);

        public bool AddEntity<T>(T entity) where T : BaseEntity
        {
            _dbContext.Set<T>().Add(entity);
            return _dbContext.SaveChanges() > 0;
        }

        public bool DeleteEntity<T>(T entity) where T : BaseEntity
        {
            _dbContext.Set<T>().Remove(entity);
            return _dbContext.SaveChanges() > 0;
        }

        public bool ExistsEntity<T>(Guid entity) where T : BaseEntity =>
            _dbContext.Set<T>().Any(t => t.Id == entity);

        public bool UpdateEntity<T>(T entity) where T : BaseEntity
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }
    }
}