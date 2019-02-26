using System;
using SpaTemplate.Core.Hateoas;

namespace SpaTemplate.Core.FacultyContext
{
    public interface ICourseService
    {
        PagedList<Course> GetPagedList<TParameters>(Guid studentId, TParameters parameters)
            where TParameters : IParameters;

        Course GetCourse(Guid studentId, Guid courseId);
        bool AddCourse(Guid studentId, Course course);
        bool DeleteCourse(Course course);
        bool UpdateCourse(Course course);
        bool StudentExists(Guid studentId);
        bool CourseMappingExists<TParameters>(TParameters parameters) where TParameters : IParameters;
        bool CoursePropertiesExists<TParameters>(TParameters parameters) where TParameters : IParameters;
    }
}