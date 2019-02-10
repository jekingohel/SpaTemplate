using System;
using System.Linq;
using System.Linq.Expressions;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
	public class RepositoryCourse : EfRepository, IRepositoryCourse<Course>
	{
		private readonly AppDbContext _dbContext;
		private readonly IPropertyMappingService _propertyMappingService;
		private readonly IRepositoryPerson<Person> _repositoryPerson;

		public RepositoryCourse(AppDbContext dbContext, IPropertyMappingService propertyMappingService,
			IRepositoryPerson<Person> repositoryPerson) :
			base(dbContext)
		{
			_dbContext = dbContext;
			_propertyMappingService = propertyMappingService;
			_repositoryPerson = repositoryPerson;
		}

		public IPagedList<Course> GetPagedList<TDto>(Guid personId, IParameters resourceParameters)
			where TDto : IDto
		{
			var courses = _dbContext.Courses.Where(x => x.Person.Id == personId);
			return _propertyMappingService.GetPagedList<TDto, Course>(courses, resourceParameters, SearchQuery);
		}

		public void AddCourse(Guid personId, Course course)
		{
			var people = _dbContext.People.FirstOrDefault(a => a.Id == personId);
			if (people == null) return;
			if (course.Id == Guid.Empty) course.Id = Guid.NewGuid();
			people.Courses.Add(course);
		}

		public bool PersonExists(Guid personId) => _repositoryPerson.EntityExists<Person>(personId);
		public Course GetCourse(Guid personId, Guid courseId)
		{
			return _dbContext.Courses
				.FirstOrDefault(b => b.Person.Id == personId && b.Id == courseId);
		}

		private static Expression<Func<Course, bool>> SearchQuery(IParameters resourceParameters) =>
			a =>
				a.Title.ToLowerInvariant()
					.Contains(resourceParameters.SearchQuery.Trim().ToLowerInvariant())
				|| a.Description.ToLowerInvariant()
					.Contains(resourceParameters.SearchQuery.Trim().ToLowerInvariant());
	}
}