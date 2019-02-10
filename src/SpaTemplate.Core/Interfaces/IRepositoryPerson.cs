using System;
using System.Collections.Generic;

namespace SpaTemplate.Core
{
	public interface IRepositoryPerson<TEntity> : IRepository where TEntity : BaseEntity
	{
		IEnumerable<TEntity> GetEntities(IEnumerable<Guid> ids);

		IPagedList<TEntity> GetPagedList<TDto>(
			IParameters resourceParameters) where TDto : IDto;
	}
}