using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Infrastructure;
using SpaTemplate.Infrastructure.Core;

namespace SpaTemplate.Web.Core
{
	[Route(Route.CoursesApi)]
	[ValidateModel]
	public class CoursesController : Controller
	{
		private readonly ICourseService _courseService;
		private readonly IUrlHelper _urlHelper;

		public CoursesController(IUrlHelper urlHelper,
			ICourseService courseService)
		{
			_urlHelper = urlHelper;
			_courseService = courseService;
		}

		[HttpGet(Name = RouteName.GetCoursesForStudent)]
		public IActionResult GetCoursesForStudent(CourseParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType,
			Guid studentId)
		{
			if (!_courseService.StudentExists(studentId)) return NotFound();
			if (!_courseService.CourseMappingExists(parameters)) return BadRequest();
			if (!_courseService.CoursePropertiesExists(parameters)) return BadRequest();
			
            var courses = _courseService.GetPagedList(studentId, parameters);
			var courseDtos = Mapper.Map<IEnumerable<CourseDto>>(courses);

			if (mediaType == MediaType.OutputFormatterJson)
			{
				Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(courses.CreateBasePagination()));
				var values = courseDtos.ShapeDataCollectionWithHateoasLinks(parameters.Fields, CreateLinksForCourse);
				var links = _urlHelper.CreateLinks(RouteName.GetCoursesForStudent, parameters, courses);
				return Ok(HateoasDto.CreateHateoasDto(values, links));
			}

			Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
				_urlHelper.CreateHateoasPagination(RouteName.GetCoursesForStudent, courses, parameters)));

			return Ok(courseDtos.ShapeDataCollection(parameters.Fields));
		}

		[HttpGet("{id}", Name = RouteName.GetCourseForStudent)]
		public IActionResult GetCourseForStudent(Guid studentId, Guid id,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!_courseService.StudentExists(studentId)) return NotFound();

			var course = _courseService.GetCourse(studentId, id);
			if (course == null) return NotFound();

			return mediaType != MediaType.OutputFormatterJson
				? Ok(course)
				: Ok(course.ShapeDataWithoutParameters<CourseDto, Course>(CreateLinksForCourse));
		}

		[HttpPost(Name = RouteName.CreateCourseForStudent)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
		public IActionResult CreateCourseForStudent(Guid studentId,
			[FromBody] CourseForCreationDto courseForCreationDto)
		{
			if (courseForCreationDto == null) return BadRequest();

			if (courseForCreationDto.Description == courseForCreationDto.Title)
				ModelState.AddModelError(nameof(CourseForCreationDto),
					"The provided description should be different from the title.");

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
			if (!_courseService.StudentExists(studentId)) return NotFound();

			var course = Mapper.Map<Course>(courseForCreationDto);

			if (!_courseService.AddCourse(studentId, course)) throw new Exception($"Creating a course for student {studentId} failed on save.");

			return CreatedAtRoute(RouteName.GetCourseForStudent,
				new { studentId, course.Id},
				course.ShapeDataWithoutParameters<CourseDto, Course>(CreateLinksForCourse));
		}

		[HttpDelete("{id}", Name = RouteName.DeleteCourseForStudent)]
		public IActionResult DeleteCourseForStudent(Guid studentId, Guid id)
		{
			if (!_courseService.StudentExists(studentId)) return NotFound();

			var course = _courseService.GetCourse(studentId, id);
			if (course == null) return NotFound();

			if (!_courseService.DeleteCourse(course))
				throw new Exception($"Deleting course {id} for student {studentId} failed on save.");

			return NoContent();
		}

		[HttpPut("{id}", Name = RouteName.UpdateCourseForStudent)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
		public IActionResult UpdateCourseForStudent(Guid studentId, Guid id,
			[FromBody] CourseForUpdateDto courseForUpdateDto)
		{
			if (courseForUpdateDto == null) return BadRequest();

			if (courseForUpdateDto.Description == courseForUpdateDto.Title)
				ModelState.AddModelError(nameof(CourseForUpdateDto),
					"The provided description should be different from the title.");

			if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
			if (!_courseService.StudentExists(studentId)) return NotFound();

			var course = _courseService.GetCourse(studentId, id);
			if (course == null)
			{
				var mapCourse = Mapper.Map<Course>(courseForUpdateDto);
				mapCourse.Id = id;

				if (!_courseService.AddCourse(studentId, mapCourse))
					throw new Exception($"Upserting course {id} for student {studentId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);

				return CreatedAtRoute(RouteName.GetCourseForStudent,
					new {studentId, courseDto.Id},
					courseDto);
			}
			Mapper.Map(courseForUpdateDto, course);
			if (!_courseService.UpdateCourse(course))
				throw new Exception($"Updating course {id} for student {studentId} failed on save.");

			return NoContent();
		}

		[HttpPatch("{id}", Name = RouteName.PartiallyUpdateCourseForStudent)]
		public IActionResult PartiallyUpdateCourseForStudent(Guid studentId, Guid id,
			[FromBody] JsonPatchDocument<CourseForUpdateDto> patchDoc)
		{
			if (patchDoc == null) return BadRequest();
			if (!_courseService.StudentExists(studentId)) return NotFound();

			var course = _courseService.GetCourse(studentId, id);

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

				if (!_courseService.AddCourse(studentId, mapCourse))
					throw new Exception($"Upserting course {id} for student {studentId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);
				return CreatedAtRoute(RouteName.GetCourseForStudent,
					new {studentId, courseDto.Id},
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

			if (!_courseService.UpdateCourse(course))
				throw new Exception($"Patching course {id} for student {studentId} failed on save.");

			return NoContent();
		}

		private string CreateHref(Guid id, string routeName, string fields = null) =>
			_urlHelper.Link(routeName, new {id, fields});

		private IEnumerable<ILinkDto> CreateLinksForCourse(Guid id, string fields = null) => new List<ILinkDto>
		{
			string.IsNullOrWhiteSpace(fields)
				? CreateHref(id, RouteName.GetCoursesForStudent)
					.AddRelAndMethod(Rel.Self, Method.Get)
				: CreateHref(id, RouteName.GetCoursesForStudent, fields)
					.AddRelAndMethod(Rel.Self, Method.Get),
			CreateHref(id, RouteName.CreateCourseForStudent)
				.AddRelAndMethod(Rel.CreateCourseForStudent, Method.Post),
			CreateHref(id, RouteName.PartiallyUpdateCourseForStudent)
				.AddRelAndMethod(Rel.PartiallyUpdateCourse, Method.Patch),
			CreateHref(id, RouteName.UpdateCourseForStudent)
				.AddRelAndMethod(Rel.UpdateCourse, Method.Put),
			CreateHref(id, RouteName.DeleteCourseForStudent)
				.AddRelAndMethod(Rel.DeleteCourse, Method.Delete)
		};
	}
}