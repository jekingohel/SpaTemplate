using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public sealed class CourseSpecification : BaseSpecification<Course>
    {
        public CourseSpecification(Guid studentId) : base(course => course.StudentId == studentId)
        {
            AddInclude(b => b.Student);
        }

        public CourseSpecification(Guid studentId, Guid courseId) : base(course =>
            CourseCriteria(course, studentId, courseId))
        {
            AddInclude(b => b.Student);
        }

        private static bool CourseCriteria(Course course, Guid studentId, Guid courseId)
        {
            if (course == null) return false;
            if (studentId != Guid.Empty && courseId != Guid.Empty)
                return course.StudentId == studentId && course.Id == courseId;
            if (studentId != Guid.Empty) return course.StudentId == studentId;
            return false;
        }
    }
}