using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class CourseSpecification : ISpecification<Course>
    {
        private readonly (Guid studentId, Guid courseId) _parameter;

        public CourseSpecification((Guid studentId, Guid courseId) parameter)
        {
            _parameter = parameter;
        }

        public Func<Course, bool> CriteriaExpression =>
            course =>
            {
                if (course == null) return false;
                if (_parameter.studentId != Guid.Empty && _parameter.courseId != Guid.Empty)
                    return course.StudentId == _parameter.studentId && course.Id == _parameter.courseId;
                if (_parameter.studentId != Guid.Empty)
                    return course.StudentId == _parameter.studentId;
                return false;
            };

        public bool IsSatisfiedBy(Course target)
        {
            return false;
        }
    }
}