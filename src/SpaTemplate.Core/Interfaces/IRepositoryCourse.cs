using System;

namespace SpaTemplate.Core
{
	public interface IRepositoryCourse<TEntity> : IRepository where TEntity : BaseEntity
	{
		bool PersonExists(Guid personId);
		Course GetCourse(Guid personId, Guid courseId);
		void AddCourse(Guid personId, Course course);

		IPagedList<TEntity> GetPagedList<TDto>(Guid personId,
			IParameters resourceParameters) where TDto : IDto;
	}
}