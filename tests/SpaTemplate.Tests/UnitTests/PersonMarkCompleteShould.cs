using System.Linq;
using SpaTemplate.Core.FacultyContext;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class PersonMarkCompleteShould
	{
		[Fact]
		public void ReturnsTrue_SetIsDone()
		{
            var sut = new Student();
            sut.MarkComplete();

			Assert.True(sut.IsDone);
		}

		[Fact]
		public void RaiseCompletedEvent()
		{
			var item = new Student();
			item.MarkComplete();

			Assert.Single(item.Events);
			Assert.IsType<StudentCompletedEvent>(item.Events.First());
		}
	}
}