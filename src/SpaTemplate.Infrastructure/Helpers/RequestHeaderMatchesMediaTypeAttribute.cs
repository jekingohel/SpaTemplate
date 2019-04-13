// -----------------------------------------------------------------------
// <copyright file="RequestHeaderMatchesMediaTypeAttribute.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc.ActionConstraints;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
	{
		private readonly string[] mediaTypes;
		private readonly string requestHeaderToMatch;

		public RequestHeaderMatchesMediaTypeAttribute(
			string requestHeaderToMatch,
			string[] mediaTypes)
		{
			this.requestHeaderToMatch = requestHeaderToMatch;
			this.mediaTypes = mediaTypes;
		}

		public int Order => 0;

		public bool Accept(ActionConstraintContext context) =>
			this.ContainsRequestHeaderIn(context) && this.MediaTypesMatches(context);

		private static IHeaderDictionary GetRequestHeaders(ActionConstraintContext context) =>
			context.RouteContext.HttpContext.Request.Headers;

		private bool ContainsRequestHeaderIn(ActionConstraintContext context) =>
			GetRequestHeaders(context).ContainsKey(this.requestHeaderToMatch);

		private bool MediaTypesMatches(ActionConstraintContext context) => this.mediaTypes.Select(mediaType =>
			GetRequestHeaders(context).Values.Select(value => Regex.Match(mediaType, value))).Any();
	}
}