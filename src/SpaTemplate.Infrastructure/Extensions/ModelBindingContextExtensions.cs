using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SpaTemplate.Infrastructure
{
	public static class ModelBindingContextExtensions
	{
		public static Task SuccessResultAsync(this ModelBindingContext bindingContext)
		{
			bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
			return Task.CompletedTask;
		}

		public static Task NullResultAsync(this ModelBindingContext bindingContext)
		{
			bindingContext.Result = ModelBindingResult.Success(null);
			return Task.CompletedTask;
		}

		public static Task FailedResultAsync(this ModelBindingContext bindingContext)
		{
			bindingContext.Result = ModelBindingResult.Failed();
			return Task.CompletedTask;
		}

		public static Array CreateArrayOfCertainType(this ModelBindingContext bindingContext) => Array.CreateInstance(
			bindingContext.ModelType.GetTypeInfo()
				.GenericTypeArguments[0], bindingContext.ConvertItemsToEnumerableType().Length);

		public static object[] ConvertItemsToEnumerableType(this ModelBindingContext bindingContext) =>
			bindingContext.InputtedValue().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => TypeDescriptor.GetConverter(bindingContext.ModelType.GetTypeInfo()
					.GenericTypeArguments[0]).ConvertFromString(x.Trim()))
				.ToArray();

		public static string InputtedValue(this ModelBindingContext bindingContext) => bindingContext.ValueProvider
			.GetValue(bindingContext.ModelName)
			.ToString();
	}
}