using System;
using System.Collections.Generic;
using System.Linq;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
	public class StudentService : IHandle<StudentCompletedEvent>, IStudentService
	{
        private readonly IRepository _repository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;

        public StudentService(IRepository repository, ITypeHelperService typeHelperService,
            IPropertyMappingService propertyMappingService)
        {
            _repository = repository;
            _typeHelperService = typeHelperService;
            _propertyMappingService = propertyMappingService;
        }

        //TODO: Create specification
        public List<Student> GetCollection(IEnumerable<Guid> ids) =>
            _repository.GetCollection<Student>().Where(a => ids.Contains(a.Id)).ToList();

        public Student GetStudent(Guid studentId) => _repository.GetEntity<Student>(studentId);

        public bool AddStudent(Student student) => _repository.AddEntity(student);
        public bool DeleteStudent(Student student) => _repository.DeleteEntity(student);
        public bool UpdateStudent(Student student) => _repository.UpdateEntity(student);
        public bool StudentExists(Guid studentId) => _repository.ExistsEntity<Student>(studentId);

        public bool StudentMappingExists<TParameters>(TParameters parameters) where TParameters : IParameters =>
            _propertyMappingService.ValidMappingExistsFor<StudentDto, Student>(
                parameters.OrderBy);

        public bool StudentPropertiesExists<TParameters>(TParameters parameters) where TParameters : IParameters =>
            _typeHelperService.TypeHasProperties<StudentDto>(parameters.Fields);

        public PagedList<Student> GetPagedList<TParameters>(TParameters parameters)
            where TParameters : IParameters => _repository.GetCollection<Student, StudentDto>(parameters,
            new StudentParametersSpecification(parameters), SpecificationQueryMode.CriteriaExpression);

		public void Handle(StudentCompletedEvent domainEvent)
		{
			if (domainEvent == null)
				throw new NullReferenceException($"Collection {nameof(domainEvent)} cannot be null");

			// Do Nothing
		}
	}
}