using System.Linq;
using SpaTemplate.Core;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class PersonMarkCompleteShould
	{
		[Theory]
		[AutoMoqData]
		public void ReturnsTrue_SetIsDone(Person item)
		{
			item.MarkComplete();

			Assert.True(item.IsDone);
		}

		[Fact]
		public void RaiseCompletedEvent()
		{
			var item = new Person();
			item.MarkComplete();

			Assert.Single(item.Events);
			Assert.IsType<PersonCompletedEvent>(item.Events.First());
		}
	}
}