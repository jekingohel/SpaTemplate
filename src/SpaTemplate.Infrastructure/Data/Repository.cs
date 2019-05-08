// -----------------------------------------------------------------------
// <copyright file="Repository.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Microsoft.EntityFrameworkCore;
	using Xeinaemm.Common;
	using Xeinaemm.Domain;
	using Xeinaemm.Hateoas;

	public class Repository : IHateoasRepository
	{
		private readonly ApplicationDbContext dbContext;
		private readonly IPropertyMappingService propertyMappingService;

		public Repository(ApplicationDbContext dbContext, IPropertyMappingService propertyMappingService)
		{
			this.dbContext = dbContext;
			this.propertyMappingService = propertyMappingService;
		}

		public bool AddEntity<T>(T entity)
			where T : BaseEntity
		{
			this.dbContext.Set<T>().Add(entity);
			return this.dbContext.SaveChanges() > 0;
		}

		public bool DeleteEntity<T>(T entity)
			where T : BaseEntity
		{
			this.dbContext.Set<T>().Remove(entity);
			return this.dbContext.SaveChanges() > 0;
		}

		public bool ExistsEntity<T>(Guid entity)
			where T : BaseEntity
			=>
			this.dbContext.Set<T>().Any(t => t.Id == entity);

		public ReadOnlyCollection<TEntity> GetCollection<TEntity>(ISpecification<TEntity> specification)
			where TEntity : BaseEntity
		{
			if (specification == null) return new ReadOnlyCollection<TEntity>(this.dbContext.Set<TEntity>().ToList());

			var queryableResultWithIncludes = specification.Includes
				.Aggregate(
					this.dbContext.Set<TEntity>().AsQueryable(),
					(current, include) => current.Include(include));

			var secondaryResult = specification.IncludeStrings
				.Aggregate(
					queryableResultWithIncludes,
					(current, include) => current.Include(include));

			return new ReadOnlyCollection<TEntity>(secondaryResult
				.Where(specification.Criteria).ToList());
		}

		public PagedListCollection<TEntity> GetCollection<TEntity>(
			ISpecification<TEntity> specification,
			IParameters parameters)
			where TEntity : BaseEntity
		{
			var entities = this.GetCollection(specification)
				.ApplySort(
					parameters.OrderBy,
					this.propertyMappingService.GetPropertyMapping<IDto, TEntity>()).ToList();

			return new PagedListCollection<TEntity>(entities, parameters.PageNumber, parameters.PageSize);
		}

		public T GetEntity<T>(Guid id)
			where T : BaseEntity
			=>
			this.dbContext.Set<T>().SingleOrDefault(e => e.Id == id);

		public TEntity GetFirstOrDefault<TEntity>(ISpecification<TEntity> specification)
			where TEntity : BaseEntity =>
			specification == null
				? this.dbContext.Set<TEntity>().FirstOrDefault()
				: this.dbContext.Set<TEntity>().FirstOrDefault(specification.Criteria);

		public bool UpdateEntity<T>(T entity)
			where T : BaseEntity
		{
			this.dbContext.Entry(entity).State = EntityState.Modified;
			return this.dbContext.SaveChanges() > 0;
		}
	}
}