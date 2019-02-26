using System;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class StudentParametersSpecification : ISpecification<Student>
    {
        private readonly IParameters _parameters;

        public StudentParametersSpecification(IParameters parameters) => _parameters = parameters;

        public Func<Student, bool> CriteriaExpression
        {
            get
            {
                if (_parameters.SearchQuery == null) return student => true;
                return student => student.Name.ToLowerInvariant()
                                      .Contains(_parameters.SearchQuery.Trim().ToLowerInvariant())
                                  || student.Surname.ToLowerInvariant()
                                      .Contains(_parameters.SearchQuery.Trim().ToLowerInvariant());
            }
        }

        public bool IsSatisfiedBy(Student target) => false;
    }
}