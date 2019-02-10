using SpaTemplate.Core;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class TypeHelperServiceShould
	{
		[Theory]
		[InlineAutoMoqData(null)]
		[InlineAutoMoqData("id")]
		[InlineAutoMoqData("Fizz")]
		[InlineAutoMoqData("id,Fizz, buzz")]
		[InlineAutoMoqData("buzz     ")]
		public void ReturnsTrue_NullOrCorrectFields(string fields, TypeHelperService sut)
		{
			Assert.True(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Theory]
		[InlineAutoMoqData("FizzBuzz!")]
		public void ReturnsFalse_FieldNotExist(string fields, TypeHelperService sut)
		{
			Assert.False(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Theory]
		[AutoMoqData]
		public void BeAssignableFromInterface(TypeHelperService sut)
		{
			Assert.IsAssignableFrom<ITypeHelperService>(sut);
		}

		private class DummyEntity : IDto
		{
			public int Id { get; set; }
			public string Fizz { get; set; }
			public string Buzz { get; set; }
		}
	}
}