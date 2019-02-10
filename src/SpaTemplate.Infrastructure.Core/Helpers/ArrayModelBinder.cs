using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SpaTemplate.Infrastructure.Core
{
	public class ArrayModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (!bindingContext.ModelMetadata.IsEnumerableType) return bindingContext.FailedResultAsync();
			if (string.IsNullOrWhiteSpace(bindingContext.InputtedValue())) return bindingContext.NullResultAsync();

			var typedValues = bindingContext.CreateArrayOfCertainType();
			bindingContext.ConvertItemsToEnumerableType().CopyTo(typedValues, 0);
			bindingContext.Model = typedValues;

			return bindingContext.SuccessResultAsync();
		}
	}
}