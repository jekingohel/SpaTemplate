using System.Linq;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.IntegrationTests
{
	public class RepositoryShould
	{
		[Fact]
		public void AddItemAndSetId()
		{
			var repository = DbContextHelper.GetRepository();
            var sut = new Student();

			Assert.True(repository.AddEntity(sut));
			var newItem = repository.GetCollection<Student>().First();

			Assert.Equal(sut.Id, newItem.Id);
		}

		[Fact]
		public void DeleteItemAfterAddingIt()
		{
			var repository = DbContextHelper.GetRepository();
            var sut = new Student();
			repository.AddEntity(sut);
			Assert.True(repository.DeleteEntity(sut));
		}

        [Fact]
		public void ExistsItemAfterAddingIt()
		{
			var repository = DbContextHelper.GetRepository();
            var sut = new Student();
			Assert.True(repository.AddEntity(sut));
			Assert.True(repository.ExistsEntity<Student>(sut.Id));
		}

		//[Theory]
		//[AutoMoqData]
		//public void UpdateItemAfterAddingIt(Student item)
		//{
		//	var repository = DbContextHelper.GetRepository();
		//	var initialTitle = Guid.NewGuid().ToString();
		//	item.Name = initialTitle;

		//	repository.AddEntity(item);
		//	Assert.True(repository.Commit());

  //          DbContextHelper.DbContext.Entry(item).State = EntityState.Detached;

		//	var newItem = repository.GetCollection<Student>()
		//		.FirstOrDefault(i => i.Name == initialTitle);
		//	Assert.NotNull(newItem);
		//	Assert.NotSame(item, newItem);
		//	var newTitle = Guid.NewGuid().ToString();
		//	newItem.Name = newTitle;

		//	repository.UpdateEntity(newItem);
		//	Assert.True(repository.Commit());

		//	var updatedItem = repository.GetCollection<Student>()
		//		.FirstOrDefault(i => i.Name == newTitle);

		//	Assert.NotNull(updatedItem);
		//	Assert.NotEqual(item.Name, updatedItem.Name);
		//	Assert.Equal(newItem.Id, updatedItem.Id);
		//}
	}
}