// -----------------------------------------------------------------------
// <copyright file="CoursesController.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
	using System;
	using System.Collections.Generic;
	using AutoMapper;
	using Microsoft.AspNetCore.JsonPatch;
	using Microsoft.AspNetCore.Mvc;
	using Newtonsoft.Json;
	using SpaTemplate.Core.FacultyContext;
	using SpaTemplate.Core.SharedKernel;

	[Route(Route.CoursesApi)]
	[ValidateModel]
	public class CoursesController : Controller
	{
		private readonly ICourseService courseService;
		private readonly IUrlHelper urlHelper;

		public CoursesController(
			IUrlHelper urlHelper,
			ICourseService courseService)
		{
			this.urlHelper = urlHelper;
			this.courseService = courseService;
		}

		[HttpPost(Name = RouteName.CreateCourseForStudent)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] { MediaType.InputFormatterJson })]
		public IActionResult CreateCourseForStudent(
			Guid studentId,
			[FromBody] CourseForCreationDto courseForCreationDto)
		{
			if (courseForCreationDto == null) return this.BadRequest();

			if (courseForCreationDto.Description == courseForCreationDto.Title)
				this.ModelState.AddModelError(nameof(CourseForCreationDto), "The provided description should be different from the title.");

			if (!this.ModelState.IsValid) return new UnprocessableEntityObjectResult(this.ModelState);
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();

			var course = Mapper.Map<Course>(courseForCreationDto);

			if (!this.courseService.AddCourse(studentId, course)) throw new Exception($"Creating a course for student {studentId} failed on save.");

			return this.CreatedAtRoute(
				RouteName.GetCourseForStudent,
				new { studentId, course.Id },
				course.ShapeDataWithoutParameters<CourseDto, Course>(this.CreateLinksForCourse));
		}

		[HttpDelete("{id}", Name = RouteName.DeleteCourseForStudent)]
		public IActionResult DeleteCourseForStudent(Guid studentId, Guid id)
		{
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();

			var course = this.courseService.GetCourse(studentId, id);
			if (course == null) return this.NotFound();

			if (!this.courseService.DeleteCourse(course))
				throw new Exception($"Deleting course {id} for student {studentId} failed on save.");

			return this.NoContent();
		}

		[HttpGet("{id}", Name = RouteName.GetCourseForStudent)]
		public IActionResult GetCourseForStudent(
			Guid studentId,
			Guid id,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();

			var course = this.courseService.GetCourse(studentId, id);
			return course == null
				? this.NotFound()
				: (IActionResult)(mediaType != MediaType.OutputFormatterJson
				? this.Ok(course)
				: this.Ok(course.ShapeDataWithoutParameters<CourseDto, Course>(this.CreateLinksForCourse)));
		}

		[HttpGet(Name = RouteName.GetCoursesForStudent)]
		public IActionResult GetCoursesForStudent(
			CourseParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType,
			Guid studentId)
		{
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();
			if (!this.courseService.CourseMappingExists(parameters)) return this.BadRequest();
			if (!this.courseService.CoursePropertiesExists(parameters)) return this.BadRequest();

			var courses = this.courseService.GetPagedList(studentId, parameters);
			var courseDtos = Mapper.Map<IEnumerable<CourseDto>>(courses);

			if (mediaType == MediaType.OutputFormatterJson)
			{
				this.Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(courses.CreateBasePagination()));
				var values = courseDtos.ShapeDataCollectionWithHateoasLinks(parameters.Fields, this.CreateLinksForCourse);
				var links = this.urlHelper.CreateLinks(RouteName.GetCoursesForStudent, parameters, courses);
				return this.Ok(HateoasDto.CreateHateoasDto(values, links));
			}

			this.Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
				this.urlHelper.CreateHateoasPagination(RouteName.GetCoursesForStudent, courses, parameters)));

			return this.Ok(courseDtos.ShapeDataCollection(parameters.Fields));
		}

		[HttpPatch("{id}", Name = RouteName.PartiallyUpdateCourseForStudent)]
		public IActionResult PartiallyUpdateCourseForStudent(
			Guid studentId,
			Guid id,
			[FromBody] JsonPatchDocument<CourseForUpdateDto> patchDoc)
		{
			if (patchDoc == null) return this.BadRequest();
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();

			var course = this.courseService.GetCourse(studentId, id);

			if (course == null)
			{
				var courseForUpdateDto = new CourseForUpdateDto();
				patchDoc.ApplyTo(courseForUpdateDto, this.ModelState);

				if (courseForUpdateDto.Description == courseForUpdateDto.Title)
					this.ModelState.AddModelError(nameof(CourseForUpdateDto), "The provided description should be different from the title.");

				_ = this.TryValidateModel(courseForUpdateDto);

				if (!this.ModelState.IsValid) return new UnprocessableEntityObjectResult(this.ModelState);

				var mapCourse = Mapper.Map<Course>(courseForUpdateDto);
				mapCourse.Id = id;

				if (!this.courseService.AddCourse(studentId, mapCourse))
					throw new Exception($"Upserting course {id} for student {studentId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);
				return this.CreatedAtRoute(
					RouteName.GetCourseForStudent,
					new { studentId, courseDto.Id },
					courseDto);
			}

			var courseToPatch = Mapper.Map<CourseForUpdateDto>(course);

			patchDoc.ApplyTo(courseToPatch, this.ModelState);

			if (courseToPatch.Description == courseToPatch.Title)
				this.ModelState.AddModelError(nameof(CourseForUpdateDto), "The provided description should be different from the title.");

			_ = this.TryValidateModel(courseToPatch);
			if (!this.ModelState.IsValid) return new UnprocessableEntityObjectResult(this.ModelState);

			_ = Mapper.Map(courseToPatch, course);

			if (!this.courseService.UpdateCourse(course))
				throw new Exception($"Patching course {id} for student {studentId} failed on save.");

			return this.NoContent();
		}

		[HttpPut("{id}", Name = RouteName.UpdateCourseForStudent)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] { MediaType.InputFormatterJson })]
		public IActionResult UpdateCourseForStudent(
			Guid studentId,
			Guid id,
			[FromBody] CourseForUpdateDto courseForUpdateDto)
		{
			if (courseForUpdateDto == null) return this.BadRequest();

			if (courseForUpdateDto.Description == courseForUpdateDto.Title)
				this.ModelState.AddModelError(nameof(CourseForUpdateDto), "The provided description should be different from the title.");

			if (!this.ModelState.IsValid) return new UnprocessableEntityObjectResult(this.ModelState);
			if (!this.courseService.StudentExists(studentId)) return this.NotFound();

			var course = this.courseService.GetCourse(studentId, id);
			if (course == null)
			{
				var mapCourse = Mapper.Map<Course>(courseForUpdateDto);
				mapCourse.Id = id;

				if (!this.courseService.AddCourse(studentId, mapCourse))
					throw new Exception($"Upserting course {id} for student {studentId} failed on save.");

				var courseDto = Mapper.Map<CourseDto>(mapCourse);

				return this.CreatedAtRoute(
					RouteName.GetCourseForStudent,
					new { studentId, courseDto.Id },
					courseDto);
			}

			_ = Mapper.Map(courseForUpdateDto, course);
			if (!this.courseService.UpdateCourse(course))
				throw new Exception($"Updating course {id} for student {studentId} failed on save.");

			return this.NoContent();
		}

		private string CreateHref(Guid id, string routeName, string fields = null) =>
			this.urlHelper.Link(routeName, new { id, fields });

		private IEnumerable<ILinkDto> CreateLinksForCourse(Guid id, string fields = null) => new List<ILinkDto>
		{
			string.IsNullOrWhiteSpace(fields)
				? this.CreateHref(id, RouteName.GetCoursesForStudent)
					.AddRelAndMethod(Rel.Self, Method.Get)
				: this.CreateHref(id, RouteName.GetCoursesForStudent, fields)
					.AddRelAndMethod(Rel.Self, Method.Get),
			this.CreateHref(id, RouteName.CreateCourseForStudent)
				.AddRelAndMethod(Rel.CreateCourseForStudent, Method.Post),
			this.CreateHref(id, RouteName.PartiallyUpdateCourseForStudent)
				.AddRelAndMethod(Rel.PartiallyUpdateCourse, Method.Patch),
			this.CreateHref(id, RouteName.UpdateCourseForStudent)
				.AddRelAndMethod(Rel.UpdateCourse, Method.Put),
			this.CreateHref(id, RouteName.DeleteCourseForStudent)
				.AddRelAndMethod(Rel.DeleteCourse, Method.Delete),
		};
	}
}