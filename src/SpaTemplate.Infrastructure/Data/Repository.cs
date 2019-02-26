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

        public List<TEntity> GetCollection<TEntity>(ISpecification<TEntity> specification = null,
            SpecificationQueryMode mode = SpecificationQueryMode.None) where TEntity : BaseEntity
        {
            if (specification == null || mode == SpecificationQueryMode.None)
                return _dbContext.Set<TEntity>().ToList();
            switch (mode)
            {
                case SpecificationQueryMode.CriteriaExpression:
                    return _dbContext.Set<TEntity>().Where(specification.CriteriaExpression).ToList();
                case SpecificationQueryMode.IsSatisfiedBy:
                    return _dbContext.Set<TEntity>().Where(specification.IsSatisfiedBy).ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Set specification of query mode");
            }
        }

        public PagedList<TEntity> GetCollection<TEntity, TDto>(IParameters parameters,
            ISpecification<TEntity> specification = null, SpecificationQueryMode mode = SpecificationQueryMode.None)
            where TEntity : BaseEntity where TDto : IDto
        {
            var entities = GetCollection(specification, mode)
                .ApplySort(parameters.OrderBy,
                    _propertyMappingService.GetPropertyMapping<TDto, TEntity>()).ToList();

            return PagedList<TEntity>.Create(entities, parameters.PageNumber, parameters.PageSize);
        }

        public TEntity GetFirstOrDefault<TEntity>(ISpecification<TEntity> specification = null,
            SpecificationQueryMode mode = SpecificationQueryMode.None) where TEntity : BaseEntity
        {
            if (specification == null || mode == SpecificationQueryMode.None)
                return _dbContext.Set<TEntity>().FirstOrDefault();
            switch (mode)
            {
                case SpecificationQueryMode.CriteriaExpression:
                    return _dbContext.Set<TEntity>().FirstOrDefault(specification.CriteriaExpression);
                case SpecificationQueryMode.IsSatisfiedBy:
                    return _dbContext.Set<TEntity>().FirstOrDefault(specification.IsSatisfiedBy);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Set specification of query mode");
            }
        }

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