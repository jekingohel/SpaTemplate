using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public sealed class StudentSpecification : BaseSpecification<Student>
    {
        public StudentSpecification(Guid studentId) : base(student => student.Id == studentId)
        {
            AddInclude(student => student.Courses);
        }
    }
}