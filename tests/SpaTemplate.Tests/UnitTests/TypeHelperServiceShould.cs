// -----------------------------------------------------------------------
// <copyright file="TypeHelperServiceShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.UnitTests
{
	using AutoFixture.Xunit2;
	using SpaTemplate.Core.SharedKernel;
	using Xunit;

	public class TypeHelperServiceShould
	{
		[Theory]
		[InlineAutoData("FizzBuzz!")]
		public void ReturnsFalse_FieldNotExist(string fields)
		{
			var sut = new TypeHelperService();
			Assert.False(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Theory]
		[InlineAutoData(null)]
		[InlineAutoData("id")]
		[InlineAutoData("Fizz")]
		[InlineAutoData("id,Fizz, buzz")]
		[InlineAutoData("buzz     ")]
		public void ReturnsTrue_NullOrCorrectFields(string fields)
		{
			var sut = new TypeHelperService();
			Assert.True(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Fact]
		public void BeAssignableFromInterface() => Assert.IsAssignableFrom<ITypeHelperService>(new TypeHelperService());

		private class DummyEntity : IDto
		{
			public string Buzz { get; set; }

			public string Fizz { get; set; }

			public int Id { get; set; }
		}
	}
}