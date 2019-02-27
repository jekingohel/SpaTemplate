using System.Collections.Generic;
using SpaTemplate.Core.SharedKernel;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class ObjectExtensionsShould
	{
		[Theory]
		[InlineData("Fizz", "Fizz")]
		[InlineData("fizz", "Fizz")]
		[InlineData("Fizz  ", "Fizz")]
		[InlineData("fizz  ", "Fizz")]
		public void ShapeData(string field, string key)
		{
            var sut = new DummyEntity();
            var list = sut.ShapeDataObject(field) as IDictionary<string, object>;

			Assert.Equal(sut.Fizz, list[key]);
			Assert.Throws<KeyNotFoundException>(() => list["Buzz"]);
			Assert.Throws<KeyNotFoundException>(() => list["Id"]);
		}

		[Fact]
		public void ReturnsEverything_FieldIsNullOrWhiteSpace()
		{
            var sut = new DummyEntity();
			var list = sut.ShapeDataObject() as IDictionary<string, object>;

			Assert.Equal(sut.Fizz, list["Fizz"]);
			Assert.Equal(sut.Buzz, list["Buzz"]);
			Assert.Equal(sut.Id, list["Id"]);
		}

		private class DummyEntity : BaseEntity
        {
            public string Fizz { get; }
			public string Buzz { get; }
		}
	}
}