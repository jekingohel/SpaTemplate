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
	[Route(Route.CoursesApi)]
	[ValidateModel]
	public class CoursesController : Controller
	{
		private readonly IPropertyMappingService _propertyMappingService;
		private readonly IRepositoryCourse<Course> _repository;
		private readonly ITypeHelperService _typeHelperService;
		private readonly IUrlHelper _urlHelper;

		public CoursesController(IUrlHelper urlHelper,
			IRepositoryCourse<Course> repository, ITypeHelperService typeHelperService,
			IPropertyMappingService propertyMappingService)
		{
			_urlHelper = urlHelper;
			_repository = repository;
			_typeHelperService = typeHelperService;
			_propertyMappingService = propertyMappingService;
		}

		[HttpGet(Name = RouteName.GetCoursesForPerson)]
		public IActionResult GetCoursesForPerson(CourseParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType,
			Guid personId)
		{
			if (!_repository.PersonExists(personId)) return NotFound();

			if (!_propertyMappingService.ValidMappingExistsFor<CourseDto, Course>(
				parameters.OrderBy)) return BadRequest();

			if (!_typeHelperService.TypeHasProperties<CourseDto>(parameters.Fields)) return BadRequest();

			var courses = _repository.GetPagedList<CourseDto>(personId, parameters);

			var courseDtos = Mapper.Map<IEnumerable<CourseDto>>(courses);
			if (mediaType == MediaType.OutputFormatterJson)
			{
				Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(courses.CreateBasePagination()));
				var values = courseDtos.GetShapedDtoWithLinks(parameters, CreateLinksForCourse);
				var links = _urlHelper.CreateLinks(RouteName.GetCoursesForPerson, parameters, courses);
				return Ok(HateoasDto.CreateHateoasDto(values, links));
			}

			Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
				_urlHelper.CreateHateoasPagination(RouteName.GetCoursesForPerson, courses, parameters)));

			return Ok(courseDtos.ShapeData(parameters.Fields));
		}

		[HttpGet("{id}", Name = RouteName.GetCourseForPerson)]
		public IActionResult GetCourseForPerson(Guid personId, Guid id,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!_repository.PersonExists(personId)) return NotFound();

			var course = _repository.GetCourse(personId, id);
			if (course == null) return NotFound();

			return mediaType != MediaType.OutputFormatterJson
				? Ok(course)
				: Ok(course.ShapeDataWithoutParameters<CourseDto, Course>(CreateLinksForCourse));
		}

		[HttpPost(Name = RouteName.CreateCourseForPerson)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
		public IActionResult CreateCourseForPerson(Guid personId,
			[FromBody] CourseForCreationDto courseForCreationDto)
		{
			if (courseForCreationDto == null) return BadRequest();

			if (courseForCreationDto.Description == courseForCreationDto.Title)
				ModelState.AddModelError(nameof(CourseForCreationDto),
					"The provided description should be different from the title.");

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

			if (!_repository.PersonExists(personId)) return NotFound();

			var course = Mapper.Map<Course>(courseForCreationDto);

			_repository.AddCourse(personId, course);

			if (!_repository.Commit()) throw new Exception($"Creating a course for person {personId} failed on save.");

			return CreatedAtRoute(RouteName.GetCourseForPerson,
				new {personId, course.Id},
				course.ShapeDataWithoutParameters<CourseDto, Course>(CreateLinksForCourse));
		}

		[HttpDelete("{id}", Name = RouteName.DeleteCourseForPerson)]
		public IActionResult DeleteCourseForPerson(Guid personId, Guid id)
		{
			if (!_repository.PersonExists(personId)) return NotFound();

			var course = _repository.GetCourse(personId, id);
			if (course == null) return NotFound();

			_repository.Delete(course);

			if (!_repository.Commit())
				throw new Exception($"Deleting course {id} for person {personId} failed on save.");

			return NoContent();
		}

		[HttpPut("{id}", Name = RouteName.UpdateCourseForPerson)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
		public IActionResult UpdateCourseForPerson(Guid personId, Guid id,
			[FromBody] CourseForUpdateDto courseForUpdateDto)
		{
			if (courseForUpdateDto == null) return BadRequest();

			if (courseForUpdateDto.Description == courseForUpdateDto.Title)
				ModelState.AddModelError(nameof(CourseForUpdateDto),
					"The provided description should be different from the title.");

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

			if (!_repository.PersonExists(personId))
				return NotFound();

			var course = _repository.GetCourse(personId, id);
			if (course == null)
			{
				var mapCourse = Mapper.Map<Course>(courseForUpdateDto);
				mapCourse.Id = id;

				_repository.AddCourse(personId, mapCourse);

				if (!_repository.Commit())
					throw new Exception($"Upserting course {id} for person {personId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);

				return CreatedAtRoute(RouteName.GetCourseForPerson,
					new {personId, courseDto.Id},
					courseDto);
			}

			Mapper.Map(courseForUpdateDto, course);

			_repository.UpdateEntity(course);

			if (!_repository.Commit())
				throw new Exception($"Updating course {id} for person {personId} failed on save.");

			return NoContent();
		}

		[HttpPatch("{id}", Name = RouteName.PartiallyUpdateCourseForPerson)]
		public IActionResult PartiallyUpdateCourseForPerson(Guid personId, Guid id,
			[FromBody] JsonPatchDocument<CourseForUpdateDto> patchDoc)
		{
			if (patchDoc == null) return BadRequest();

			if (!_repository.PersonExists(personId)) return NotFound();

			var course = _repository.GetCourse(personId, id);

			if (course == null)
			{
				var courseForUpdateDto = new CourseForUpdateDto();
				patchDoc.ApplyTo(courseForUpdateDto, ModelState);

				if (courseForUpdateDto.Description == courseForUpdateDto.Title)
					ModelState.AddModelError(nameof(CourseForUpdateDto),
						"The provided description should be different from the title.");

				TryValidateModel(courseForUpdateDto);

				if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

				var mapCourse = Mapper.Map<Course>(courseForUpdateDto);
				mapCourse.Id = id;

				_repository.AddCourse(personId, mapCourse);

				if (!_repository.Commit())
					throw new Exception($"Upserting course {id} for person {personId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);
				return CreatedAtRoute(RouteName.GetCourseForPerson,
					new {personId, courseDto.Id},
					courseDto);
			}

			var courseToPatch = Mapper.Map<CourseForUpdateDto>(course);

			patchDoc.ApplyTo(courseToPatch, ModelState);

			if (courseToPatch.Description == courseToPatch.Title)
				ModelState.AddModelError(nameof(CourseForUpdateDto),
					"The provided description should be different from the title.");

			TryValidateModel(courseToPatch);

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

			Mapper.Map(courseToPatch, course);

			_repository.UpdateEntity(course);

			if (!_repository.Commit())
				throw new Exception($"Patching course {id} for person {personId} failed on save.");

			return NoContent();
		}

		private string CreateHref(Guid id, string routeName, string fields = null) =>
			_urlHelper.Link(routeName, new {id, fields});

		private IEnumerable<ILinkDto> CreateLinksForCourse(Guid id, string fields = null) => new List<ILinkDto>
		{
			string.IsNullOrWhiteSpace(fields)
				? CreateHref(id, RouteName.GetCoursesForPerson)
					.AddRelAndMethod(Rel.Self, Method.Get)
				: CreateHref(id, RouteName.GetCoursesForPerson, fields)
					.AddRelAndMethod(Rel.Self, Method.Get),
			CreateHref(id, RouteName.CreateCourseForPerson)
				.AddRelAndMethod(Rel.CreateCourseForPerson, Method.Post),
			CreateHref(id, RouteName.PartiallyUpdateCourseForPerson)
				.AddRelAndMethod(Rel.PartiallyUpdateCourse, Method.Patch),
			CreateHref(id, RouteName.UpdateCourseForPerson)
				.AddRelAndMethod(Rel.UpdateCourse, Method.Put),
			CreateHref(id, RouteName.DeleteCourseForPerson)
				.AddRelAndMethod(Rel.DeleteCourse, Method.Delete)
		};
	}
}