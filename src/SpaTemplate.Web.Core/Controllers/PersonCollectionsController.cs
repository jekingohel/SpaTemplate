using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SpaTemplate.Core;
using SpaTemplate.Infrastructure.Core;

namespace SpaTemplate.Web.Core
{
	[Route(Route.PersonCollectionsApi)]
	public class PeopleCollectionsController : Controller
	{
		private readonly IRepositoryPerson<Person> _repository;

		public PeopleCollectionsController(IRepositoryPerson<Person> todo) =>
			_repository = todo;

		[HttpGet("({ids})", Name = RouteName.GetPersonCollection)]
		public IActionResult GetPersonCollection(
			[ModelBinder(BinderType = typeof(ArrayModelBinder))]
			IEnumerable<Guid> ids)
		{
			if (ids == null) return BadRequest();

			var collection = ids.ToList();
			var entities = _repository.GetEntities(collection);

			if (collection.Count != entities.Count()) return NotFound();
			return Ok(Mapper.Map<IEnumerable<PersonDto>>(entities));
		}

		[HttpPost]
		public IActionResult CreatePersonCollection(
			[FromBody] IEnumerable<PersonForCreationDto> personForCreationDtos)
		{
			if (personForCreationDtos == null) return BadRequest();
			var people = Mapper.Map<IEnumerable<Person>>(personForCreationDtos);

			foreach (var person in people) _repository.Add(person);
			if (!_repository.Commit()) throw new Exception("Creating an People collection failed on save.");

			var peopleDto = Mapper.Map<IEnumerable<PersonDto>>(people);
			return CreatedAtRoute(RouteName.GetPersonCollection,
				new
				{
					ids = string.Join(",", peopleDto.Select(a => a.Id))
				}, peopleDto);
		}
	}
}