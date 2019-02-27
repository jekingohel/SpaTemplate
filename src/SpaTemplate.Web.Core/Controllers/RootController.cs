using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Web.Core
{
	[Route(Route.RootApi)]
	public class RootController : Controller
	{
		private readonly IUrlHelper _urlHelper;

		public RootController(IUrlHelper urlHelper) => _urlHelper = urlHelper;

		[HttpGet(Name = RouteName.GetRoot)]
		public IActionResult GetRoot([FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (mediaType != MediaType.OutputFormatterJson) return NoContent();
			return Ok(RootDtoLinks());
		}

		private List<ILinkDto> RootDtoLinks() => new List<ILinkDto>
		{
			CreateHref(RouteName.GetRoot)
				.AddRelAndMethod(Rel.Self, Method.Get),
			CreateHref(RouteName.GetPeople)
				.AddRelAndMethod(Rel.People, Method.Get),
			CreateHref(RouteName.CreateStudent)
				.AddRelAndMethod(Rel.CreateStudent, Method.Post)
		};

		private string CreateHref(string routeName) => _urlHelper.Link(routeName, new { });
	}
}