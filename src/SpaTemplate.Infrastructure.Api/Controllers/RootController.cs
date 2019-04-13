// -----------------------------------------------------------------------
// <copyright file="RootController.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
	using System.Collections.Generic;
	using Microsoft.AspNetCore.Mvc;
	using SpaTemplate.Core.SharedKernel;

	[Route(Route.RootApi)]
	public class RootController : Controller
	{
		private readonly IUrlHelper urlHelper;

		public RootController(IUrlHelper urlHelper) => this.urlHelper = urlHelper;

		[HttpGet(Name = RouteName.GetRoot)]
		public IActionResult GetRoot([FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (mediaType != MediaType.OutputFormatterJson) return this.NoContent();
			return this.Ok(this.RootDtoLinks());
		}

		private string CreateHref(string routeName) => this.urlHelper.Link(routeName, new { });

		private List<ILinkDto> RootDtoLinks() => new List<ILinkDto>
		{
			this.CreateHref(RouteName.GetRoot)
				.AddRelAndMethod(Rel.Self, Method.Get),
			this.CreateHref(RouteName.GetPeople)
				.AddRelAndMethod(Rel.People, Method.Get),
			this.CreateHref(RouteName.CreateStudent)
				.AddRelAndMethod(Rel.CreateStudent, Method.Post),
		};
	}
}