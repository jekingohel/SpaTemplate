using System.Collections.Generic;
using System.Linq;
using SpaTemplate.Core;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class IEnumerableExtensionsShould
	{
		[Theory]
		[InlineData("Fizz", "Fizz", 0, "Fizz0")]
		[InlineData("fizz", "Fizz", 0, "Fizz0")]
		[InlineData("Fizz ", "Fizz", 0, "Fizz0")]
		[InlineData("fizz ", "Fizz", 0, "Fizz0")]
		public void ShapeData(string field, string key, int objectNumber, string expectedValue)
		{
			var list = DummyList().ShapeData(field).Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal(expectedValue, list[objectNumber][key]);
			Assert.Throws<KeyNotFoundException>(() => list[objectNumber]["Buzz"]);
			Assert.Throws<KeyNotFoundException>(() => list[objectNumber]["Id"]);
		}

		private static IEnumerable<DummyEntity> DummyList() => new[]
		{
			new DummyEntity {Id = 0, Fizz = "Fizz0", Buzz = "Buzz0"},
			new DummyEntity {Id = 1, Fizz = "Fizz1", Buzz = "Buzz1"},
			new DummyEntity {Id = 2, Fizz = "Fizz2", Buzz = "Buzz2"},
			new DummyEntity {Id = 3, Fizz = "Fizz3", Buzz = "Buzz3"},
			new DummyEntity {Id = 4, Fizz = "Fizz4", Buzz = "Buzz4"}
		};

		private class DummyEntity
		{
			public string Fizz { get; set; }
			public string Buzz { get; set; }
			public int Id { get; set; }
		}

		[Fact]
		public void ReturnsEverything_FieldIsNullOrWhiteSpace()
		{
			var list = DummyList().ShapeData(null).Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal("Fizz0", list.First()["Fizz"]);
			Assert.Equal("Buzz0", list.First()["Buzz"]);
			Assert.Equal(0, list.First()["Id"]);
		}
	}
}