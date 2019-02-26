using System;

namespace SpaTemplate.Core.SharedKernel
{
    public interface ISpecification<in T>
    {
        Func<T, bool> CriteriaExpression { get; }
        bool IsSatisfiedBy(T target);
    }
}
