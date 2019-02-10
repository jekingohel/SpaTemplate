namespace SpaTemplate.Core
{
	public interface IUnityOfWork
	{
		bool Commit();
	}
}