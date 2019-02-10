using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
	public class RepositoryPerson : EfRepository, IRepositoryPerson<Person>
	{
		private readonly AppDbContext _dbContext;
		private readonly IPropertyMappingService _propertyMappingService;

		public RepositoryPerson(AppDbContext dbContext, IPropertyMappingService propertyMappingService) :
			base(dbContext)
		{
			_dbContext = dbContext;
			_propertyMappingService = propertyMappingService;
		}

		public IPagedList<Person> GetPagedList<TDto>(IParameters resourceParameters) where TDto : IDto => 
			_propertyMappingService.GetPagedList<TDto, Person>(_dbContext.People, resourceParameters, SearchQuery);

		public IEnumerable<Person> GetEntities(IEnumerable<Guid> ids) => 
			_dbContext.People.Where(a => ids.Contains(a.Id));

		private static Expression<Func<Person, bool>> SearchQuery(IParameters resourceParameters) => a =>
			a.Name.ToLowerInvariant()
				.Contains(resourceParameters.SearchQuery.Trim().ToLowerInvariant())
			|| a.Surname.ToLowerInvariant()
				.Contains(resourceParameters.SearchQuery.Trim().ToLowerInvariant());
	}
}