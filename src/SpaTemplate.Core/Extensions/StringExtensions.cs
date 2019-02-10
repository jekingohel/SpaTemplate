namespace SpaTemplate.Core
{
	public static class StringExtensions
	{
		public static LinkDto AddRelAndMethod(this string href, string relName,
			string methodName) =>
			new LinkDto(href, relName, methodName);
	}
}