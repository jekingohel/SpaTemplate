// -----------------------------------------------------------------------
// <copyright file="IEnumerableExtensionsShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using SpaTemplate.Core.SharedKernel;
	using Xunit;

	public class IEnumerableExtensionsShould
	{
		[Fact]
		public void ReturnsEverything_FieldIsNullOrWhiteSpace()
		{
			var list = DummyList().ShapeDataCollection().Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal("Fizz0", list[0]?["Fizz"]);
			Assert.Equal("Buzz0", list[0]?["Buzz"]);
		}

		[Theory]
		[InlineData("Fizz", "Fizz", 0, "Fizz0")]
		[InlineData("fizz", "Fizz", 0, "Fizz0")]
		[InlineData("Fizz ", "Fizz", 0, "Fizz0")]
		[InlineData("fizz ", "Fizz", 0, "Fizz0")]
		public void ShapeData(string field, string key, int objectNumber, string expectedValue)
		{
			var list = DummyList().ShapeDataCollection(field).Select(item => item as IDictionary<string, object>)
				.ToList();

			Assert.Equal(expectedValue, list[objectNumber]?[key]);
			_ = Assert.Throws<KeyNotFoundException>(() => list[objectNumber]?["Buzz"]);
			_ = Assert.Throws<KeyNotFoundException>(() => list[objectNumber]?["Id"]);
		}

		private static IEnumerable<DummyEntity> DummyList() => new List<DummyEntity>
		{
			new DummyEntity { Id = Guid.NewGuid(), Fizz = "Fizz0", Buzz = "Buzz0" },
			new DummyEntity { Id = Guid.NewGuid(), Fizz = "Fizz1", Buzz = "Buzz1" },
			new DummyEntity { Id = Guid.NewGuid(), Fizz = "Fizz2", Buzz = "Buzz2" },
			new DummyEntity { Id = Guid.NewGuid(), Fizz = "Fizz3", Buzz = "Buzz3" },
			new DummyEntity { Id = Guid.NewGuid(), Fizz = "Fizz4", Buzz = "Buzz4" },
		};

		private class DummyEntity : BaseEntity
		{
			public string Buzz { get; set; }

			public string Fizz { get; set; }
		}
	}
}