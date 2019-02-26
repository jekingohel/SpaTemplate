using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class StudentSpecification : ISpecification<Student>
    {
        private readonly Guid _studentId;

        public StudentSpecification(Guid studentId)
        {
            _studentId = studentId;
        }

        public bool IsSatisfiedBy(Student target) => target.Id == _studentId;

        Func<Student, bool> ISpecification<Student>.CriteriaExpression => student => false;
    }
}