using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public sealed class CourseParametersSpecification : BaseSpecification<Course>
    {
        public CourseParametersSpecification(IParameters parameters, Guid studentId) : base(course =>
            CriteriaExpression(course, parameters, studentId))
        {
            AddInclude(course => course.Student);
        }

        private static bool CriteriaExpression(Course course, IParameters parameters, Guid studentId)
        {
            if (parameters.SearchQuery == null) return true;
            return course.Student.Id == studentId &&
                   (course.Title.ToLowerInvariant()
                        .Contains(parameters.SearchQuery.Trim().ToLowerInvariant())
                    || course.Description.ToLowerInvariant()
                        .Contains(parameters.SearchQuery.Trim().ToLowerInvariant()));
        }
    }
}