using System.Collections.Generic;
using SpaTemplate.Core;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class ObjectExtensionsShould
	{
		[Theory]
		[InlineAutoMoqData("Fizz", "Fizz")]
		[InlineAutoMoqData("fizz", "Fizz")]
		[InlineAutoMoqData("Fizz  ", "Fizz")]
		[InlineAutoMoqData("fizz  ", "Fizz")]
		public void ShapeData(string field, string key, DummyEntity sut)
		{
			var list = sut.ShapeData(field) as IDictionary<string, object>;

			Assert.Equal(sut.Fizz, list[key]);
			Assert.Throws<KeyNotFoundException>(() => list["Buzz"]);
			Assert.Throws<KeyNotFoundException>(() => list["Id"]);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsEverything_FieldIsNullOrWhiteSpace(DummyEntity sut)
		{
			var list = sut.ShapeData(null) as IDictionary<string, object>;

			Assert.Equal(sut.Fizz, list["Fizz"]);
			Assert.Equal(sut.Buzz, list["Buzz"]);
			Assert.Equal(sut.Id, list["Id"]);
		}

		public class DummyEntity
		{
			public string Fizz { get; set; }
			public string Buzz { get; set; }
			public int Id { get; set; }
		}
	}
}