using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public sealed class StudentParametersSpecification : BaseSpecification<Student>
    {
        public StudentParametersSpecification(Guid studentId) : base(student => student.Id == studentId)
        {
            AddInclude(student => student.Courses);
        }

        public StudentParametersSpecification(IParameters parameters) : base(student =>
            CriteriaExpression(student, parameters))
        {
            AddInclude(student => student.Courses);
        }

        private static bool CriteriaExpression(Student student, IParameters parameters)
        {
            if (parameters.SearchQuery == null) return true;
            return student.Name.ToLowerInvariant()
                       .Contains(parameters.SearchQuery.Trim().ToLowerInvariant())
                   || student.Surname.ToLowerInvariant()
                       .Contains(parameters.SearchQuery.Trim().ToLowerInvariant());
        }
    }
}