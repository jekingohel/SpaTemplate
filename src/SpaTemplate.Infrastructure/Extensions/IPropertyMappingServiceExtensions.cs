using System;
using System.Linq;
using System.Linq.Expressions;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
	public static class IPropertyMappingServiceExtensions
	{
		public static PagedList<TEntity> GetPagedList<TDto, TEntity>(
			this IPropertyMappingService propertyMappingService, IQueryable<TEntity> entity,
			IParameters parameters, Func<IParameters, Expression<Func<TEntity, bool>>> function)
			where TDto : IDto where TEntity : BaseEntity
		{
			var collectionBeforePaging = entity
				.ApplySort(parameters.OrderBy,
					propertyMappingService.GetPropertyMapping<TDto, TEntity>());

			return PagedList<TEntity>.Create(string.IsNullOrEmpty(parameters.SearchQuery)
					? collectionBeforePaging
					: collectionBeforePaging.Where(function(parameters)),
				parameters.PageNumber, parameters.PageSize);
		}
	}
}