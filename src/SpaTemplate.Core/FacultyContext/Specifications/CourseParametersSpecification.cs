using System;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class CourseParametersSpecification : ISpecification<Course>
    {
        private readonly IParameters _parameters;
        private readonly Guid _studentId;

        public CourseParametersSpecification(IParameters parameters, Guid studentId)
        {
            _parameters = parameters;
            _studentId = studentId;
        }

        public Func<Course, bool> CriteriaExpression
        {
            get
            {
                if (_parameters.SearchQuery == null) return course => true;
                return course => course.Student.Id == _studentId &&
                                 (course.Title.ToLowerInvariant()
                                      .Contains(_parameters.SearchQuery.Trim().ToLowerInvariant())
                                  || course.Description.ToLowerInvariant()
                                      .Contains(_parameters.SearchQuery.Trim().ToLowerInvariant()));
            }
        }

        public bool IsSatisfiedBy(Course target) => false;
    }
}