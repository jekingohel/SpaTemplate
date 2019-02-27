using System;
using System.Collections.Generic;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public interface IStudentService
    {
        List<Student> GetCollection(IEnumerable<Guid> ids);
        Student GetStudent(Guid studentId);
        bool AddStudent(Student student);
        bool DeleteStudent(Student student);
        bool UpdateStudent(Student student);
        bool StudentExists(Guid studentId);
        bool StudentMappingExists<TParameters>(TParameters parameters) where TParameters : IParameters;
        bool StudentPropertiesExists<TParameters>(TParameters parameters) where TParameters : IParameters;

        PagedList<Student> GetPagedList<TParameters>(TParameters parameters)
            where TParameters : IParameters;
    }
}