using System.Linq;
using System.Reflection;

namespace SpaTemplate.Core.SharedKernel
{
	public class TypeHelperService : ITypeHelperService
	{
		public bool TypeHasProperties<T>(string fields) where T : IDto
		{
			if (string.IsNullOrWhiteSpace(fields)) return true;

			return fields.Split(',').Select(field => field.Trim())
				.Select(propertyName => typeof(T).GetProperty(propertyName,
					PublicInstances()))
				.All(propertyInfo => propertyInfo != null);
		}

		private static BindingFlags PublicInstances() =>
			BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
	}
}