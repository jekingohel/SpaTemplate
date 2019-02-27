using System;
using System.Collections.Generic;
using System.Linq;
using SpaTemplate.Core.SharedKernel;
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
            var list = DummyList().ShapeDataCollection(field).Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal(expectedValue, list[objectNumber][key]);
			Assert.Throws<KeyNotFoundException>(() => list[objectNumber]["Buzz"]);
			Assert.Throws<KeyNotFoundException>(() => list[objectNumber]["Id"]);
		}

		private static IEnumerable<DummyEntity> DummyList() => new List<DummyEntity>
		{
			new DummyEntity {Id = Guid.NewGuid(), Fizz = "Fizz0", Buzz = "Buzz0"},
			new DummyEntity {Id = Guid.NewGuid(), Fizz = "Fizz1", Buzz = "Buzz1"},
			new DummyEntity {Id = Guid.NewGuid(), Fizz = "Fizz2", Buzz = "Buzz2"},
			new DummyEntity {Id = Guid.NewGuid(), Fizz = "Fizz3", Buzz = "Buzz3"},
			new DummyEntity {Id = Guid.NewGuid(), Fizz = "Fizz4", Buzz = "Buzz4"}
		};

		private class DummyEntity : BaseEntity
		{
			public string Fizz { get; set; }
			public string Buzz { get; set; }
		}

		[Fact]
		public void ReturnsEverything_FieldIsNullOrWhiteSpace()
		{
			var list = DummyList().ShapeDataCollection().Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal("Fizz0", list.First()["Fizz"]);
			Assert.Equal("Buzz0", list.First()["Buzz"]);
		}
	}
}