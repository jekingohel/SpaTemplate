using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpaTemplate.Core;
using SpaTemplate.Infrastructure;
using SpaTemplate.Infrastructure.Core;

namespace SpaTemplate.Web.Core
{
	[Route(Route.PeopleApi)]
	[ValidateModel]
	public partial class PeopleController : Controller
	{
		private readonly IPropertyMappingService _propertyMappingService;
		private readonly IRepositoryPerson<Person> _repository;
		private readonly ITypeHelperService _typeHelperService;
		private readonly IUrlHelper _urlHelper;

		public PeopleController(IPropertyMappingService propertyMappingService,
			IRepositoryPerson<Person> repository,
			ITypeHelperService typeHelperService, IUrlHelper urlHelper)
		{
			_propertyMappingService = propertyMappingService;
			_repository = repository;
			_typeHelperService = typeHelperService;
			_urlHelper = urlHelper;
		}

		[HttpGet(Name = RouteName.GetPeople)]
		public IActionResult GetPeople(PersonParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!_propertyMappingService.ValidMappingExistsFor<PersonDto, Person>(parameters.OrderBy))
				return BadRequest();

			if (!_typeHelperService.TypeHasProperties<PersonDto>(parameters.Fields)) return BadRequest();

			var people = _repository.GetPagedList<PersonDto>(parameters);

			var personDtos = Mapper.Map<IEnumerable<PersonDto>>(people);

			if (mediaType == MediaType.OutputFormatterJson)
			{
				Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(people.CreateBasePagination()));
				var values = personDtos.GetShapedDtoWithLinks(parameters, CreateLinksPerson);
				var links = _urlHelper.CreateLinks(RouteName.GetPeople, parameters, people);
				return Ok(HateoasDto.CreateHateoasDto(values, links));
			}

			Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
				_urlHelper.CreateHateoasPagination(RouteName.GetPeople, people, parameters)));

			return Ok(personDtos.ShapeData(parameters.Fields));
		}

		[HttpGet("{id}", Name = RouteName.GetPerson)]
		public IActionResult GetPerson(Guid id,
			[FromHeader(Name = Header.Accept)] string mediaType,
			[FromQuery] string fields)
		{
			if (!_typeHelperService.TypeHasProperties<PersonDto>(fields)) return BadRequest();

			var person = _repository.GetById<Person>(id);

			if (person == null) return NotFound();

			return mediaType == MediaType.OutputFormatterJson
				? Ok(person.ShapeDataWithoutParameters<PersonDto, Person>(CreateLinksPerson))
				: Ok(Mapper.Map<PersonDto>(person));
		}

		[HttpPost(Name = RouteName.CreatePerson)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
		public IActionResult CreatePerson([FromBody] PersonForCreationDto personForCreationDto)
		{
			if (personForCreationDto == null) return BadRequest();

			var person = Mapper.Map<Person>(personForCreationDto);
			_repository.Add(person);

			if (!_repository.Commit()) throw new Exception("Creating an Person failed on save.");

			return CreatedAtRoute(RouteName.GetPerson, new {id = person.Id},
				person.ShapeDataWithoutParameters<PersonDto, Person>(CreateLinksPerson));
		}

		[HttpPatch("{id}", Name = RouteName.PartiallyUpdatePerson)]
		public IActionResult PartiallyUpdatePerson(Guid id,
			[FromBody] JsonPatchDocument<PersonForUpdateDto> patchDoc)
		{
			if (patchDoc == null) return BadRequest();
			if (!_repository.EntityExists<Person>(id)) return NotFound();

			var person = _repository.GetById<Person>(id);
			var personForUpdateDto = Mapper.Map<PersonForUpdateDto>(person);

			patchDoc.ApplyTo(personForUpdateDto, ModelState);

			if (personForUpdateDto.Name == personForUpdateDto.Surname)
				ModelState.AddModelError(nameof(PersonForUpdateDto),
					"The provided surname should be different from the name.");

			TryValidateModel(personForUpdateDto);

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

			Mapper.Map(personForUpdateDto, person);
			_repository.UpdateEntity(person);

			if (!_repository.Commit()) throw new Exception($"Patching person {id} failed on save.");

			return NoContent();
		}


		[HttpDelete("{id}", Name = RouteName.DeletePerson)]
		public IActionResult DeletePerson(Guid id)
		{
			var person = _repository.GetById<Person>(id);
			if (person == null) return NotFound();

			_repository.Delete(person);

			if (!_repository.Commit()) throw new Exception($"Deleting Person {id} failed on save.");

			return NoContent();
		}
	}

	public partial class PeopleController
	{
		private string CreateHref(Guid id, string routeName, string fields = null) =>
			_urlHelper.Link(routeName, new {id, fields});

		private IEnumerable<ILinkDto> CreateLinksPerson(Guid id, string fields = null) => new List<ILinkDto>
		{
			string.IsNullOrWhiteSpace(fields)
				? CreateHref(id, RouteName.GetPerson)
					.AddRelAndMethod(Rel.Self, Method.Get)
				: CreateHref(id, RouteName.GetPerson, fields)
					.AddRelAndMethod(Rel.Self, Method.Get),
			CreateHref(id, RouteName.CreatePerson)
				.AddRelAndMethod(Rel.CreatePerson, Method.Post),
			CreateHref(id, RouteName.PartiallyUpdatePerson)
				.AddRelAndMethod(Rel.PatchPerson, Method.Patch),
			CreateHref(id, RouteName.DeletePerson)
				.AddRelAndMethod(Rel.DeletePerson, Method.Delete)
		};
	}
}