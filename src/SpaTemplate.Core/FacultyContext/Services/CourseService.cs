using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class CourseService : IHandle<CourseCompletedEvent>, ICourseService
    {
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IRepository _repository;
        private readonly ITypeHelperService _typeHelperService;

        public CourseService(IRepository repository, IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _repository = repository;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        public bool AddCourse(Guid studentId, Course course)
        {
            var people = _repository.GetFirstOrDefault(new StudentSpecification(studentId));
            if (people == null) return false;
            if (course.Id == Guid.Empty) course.Id = Guid.NewGuid();
            return _repository.AddEntity(course);
        }

        public bool DeleteCourse(Course course) => _repository.DeleteEntity(course);
        public bool UpdateCourse(Course course) => _repository.UpdateEntity(course);
        public bool StudentExists(Guid studentId) => _repository.ExistsEntity<Student>(studentId);

        public PagedList<Course> GetPagedList<TParameters>(Guid studentId, TParameters parameters)
            where TParameters : IParameters =>
            _repository.GetCollection<Course, CourseDto>(
                new CourseParametersSpecification(parameters, studentId), parameters);

        public Course GetCourse(Guid studentId, Guid courseId) =>
            _repository.GetFirstOrDefault(new CourseSpecification(studentId, courseId));

        public bool CourseMappingExists<TParameters>(TParameters parameters) where TParameters : IParameters =>
            _propertyMappingService.ValidMappingExistsFor<CourseDto, Course>(
                parameters.OrderBy);

        public bool CoursePropertiesExists<TParameters>(TParameters parameters) where TParameters : IParameters =>
            _typeHelperService.TypeHasProperties<CourseDto>(parameters.Fields);

        public void Handle(CourseCompletedEvent domainEvent)
        {
            if (domainEvent == null)
                throw new NullReferenceException($"Collection {nameof(domainEvent)} cannot be null");

            // Do Nothing
        }
    }
}