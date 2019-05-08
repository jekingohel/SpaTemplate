// -----------------------------------------------------------------------
// <copyright file="RootController.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
	using System.Collections.Generic;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Net.Http.Headers;
	using SpaTemplate.Core.SharedKernel;
	using Xeinaemm.AspNetCore;
	using Xeinaemm.Hateoas;

	/// <summary>
	///
	/// </summary>
	[Route(Route.RootApi)]
	[ValidateModel]
	[ApiController]
	public class RootController : Controller
	{
		private readonly IUrlHelper urlHelper;

		/// <summary>
		/// Initializes a new instance of the <see cref="RootController"/> class.
		/// </summary>
		/// <param name="urlHelper"></param>
		public RootController(IUrlHelper urlHelper) => this.urlHelper = urlHelper;

		/// <summary>
		///
		/// </summary>
		/// <param name="mediaType"></param>
		/// <returns>
		///
		/// </returns>
		[HttpGet(Name = RouteName.GetRoot)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesDefaultResponseType]
		public IActionResult GetRoot([FromHeader(Name = HeaderNames.Accept)] string mediaType) =>
			mediaType != MediaType.OutputFormatterJson ? this.NoContent() : (IActionResult)this.Ok(this.RootDtoLinks());

		private string CreateHref(string routeName) => this.urlHelper.Link(routeName, new { });

		private List<LinkDto> RootDtoLinks() => new List<LinkDto>
		{
			this.CreateHref(RouteName.GetRoot)
				.AddRelAndMethod(Rel.Self, HttpMethods.Get),
			this.CreateHref(RouteName.GetPeople)
				.AddRelAndMethod(SpaTemplateRel.People, HttpMethods.Get),
			this.CreateHref(RouteName.CreateStudent)
				.AddRelAndMethod(SpaTemplateRel.CreateStudent, HttpMethods.Post),
		};
	}
}