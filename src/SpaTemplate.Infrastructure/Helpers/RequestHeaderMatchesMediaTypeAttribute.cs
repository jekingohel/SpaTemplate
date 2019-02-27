using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace SpaTemplate.Infrastructure
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
	{
		private readonly string[] _mediaTypes;
		private readonly string _requestHeaderToMatch;

		public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
			string[] mediaTypes)
		{
			_requestHeaderToMatch = requestHeaderToMatch;
			_mediaTypes = mediaTypes;
		}

		public int Order => 0;

		public bool Accept(ActionConstraintContext context) =>
			ContainsRequestHeaderIn(context) && MediaTypesMatches(context);

		private bool MediaTypesMatches(ActionConstraintContext context) => _mediaTypes.Select(mediaType =>
			GetRequestHeaders(context).Values.Select(value => Regex.Match(mediaType, value))).Any();

		private bool ContainsRequestHeaderIn(ActionConstraintContext context) =>
			GetRequestHeaders(context).ContainsKey(_requestHeaderToMatch);

		private static IHeaderDictionary GetRequestHeaders(ActionConstraintContext context) =>
			context.RouteContext.HttpContext.Request.Headers;
	}
}