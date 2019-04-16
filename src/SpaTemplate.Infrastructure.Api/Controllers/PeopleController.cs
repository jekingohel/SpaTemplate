// -----------------------------------------------------------------------
// <copyright file="PeopleController.cs" company="Piotr Xeinaemm Czech">
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

	[Route(Route.PeopleApi)]
	[ValidateModel]
	public partial class PeopleController : Controller
	{
		private readonly IStudentService studentService;
		private readonly IUrlHelper urlHelper;

		public PeopleController(
			IStudentService studentService, IUrlHelper urlHelper)
		{
			this.studentService = studentService;
			this.urlHelper = urlHelper;
		}

		[HttpPost(Name = RouteName.CreateStudent)]
		[RequestHeaderMatchesMediaType(Header.ContentType, new[] { MediaType.InputFormatterJson })]
		public IActionResult CreateStudent([FromBody] StudentForCreationDto studentForCreationDto)
		{
			if (studentForCreationDto == null) return this.BadRequest();

			var student = Mapper.Map<Student>(studentForCreationDto);
			if (!this.studentService.AddStudent(student)) throw new Exception("Creating an Student failed on save.");
			return this.CreatedAtRoute(
				RouteName.GetStudent,
				new { id = student.Id },
				student.ShapeDataWithoutParameters<StudentDto, Student>(this.CreateLinksStudent));
		}

		[HttpDelete("{id}", Name = RouteName.DeleteStudent)]
		public IActionResult DeleteStudent(Guid id)
		{
			var student = this.studentService.GetStudent(id);
			if (student == null) return this.NotFound();

			if (!this.studentService.DeleteStudent(student)) throw new Exception($"Deleting Student {id} failed on save.");
			return this.NoContent();
		}

		[HttpGet(Name = RouteName.GetPeople)]
		public IActionResult GetPeople(
			StudentParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!this.studentService.StudentMappingExists(parameters))
				return this.BadRequest();
			if (!this.studentService.StudentPropertiesExists(parameters))
				return this.BadRequest();

			var people = this.studentService.GetPagedList(parameters);
			var studentDtos = Mapper.Map<List<StudentDto>>(people);

			if (mediaType == MediaType.OutputFormatterJson)
			{
				this.Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(people.CreateBasePagination()));
				var values = studentDtos.ShapeDataCollectionWithHateoasLinks(parameters.Fields, this.CreateLinksStudent);
				var links = this.urlHelper.CreateLinks(RouteName.GetPeople, parameters, people);
				return this.Ok(HateoasDto.CreateHateoasDto(values, links));
			}

			this.Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
				this.urlHelper.CreateHateoasPagination(RouteName.GetPeople, people, parameters)));

			return this.Ok(studentDtos.ShapeDataCollection(parameters.Fields));
		}

		[HttpGet("{id}", Name = RouteName.GetStudent)]
		public IActionResult GetStudent(
			Guid id,
			StudentParameters parameters,
			[FromHeader(Name = Header.Accept)] string mediaType)
		{
			if (!this.studentService.StudentPropertiesExists(parameters)) return this.BadRequest();

			var student = this.studentService.GetStudent(id);
			return student == null
				? this.NotFound()
				: (IActionResult)(mediaType == MediaType.OutputFormatterJson
				? this.Ok(student.ShapeDataWithoutParameters<StudentDto, Student>(this.CreateLinksStudent))
				: this.Ok(Mapper.Map<StudentDto>(student)));
		}

		[HttpPatch("{id}", Name = RouteName.PartiallyUpdateStudent)]
		public IActionResult PartiallyUpdateStudent(
			Guid id,
			[FromBody] JsonPatchDocument<StudentForUpdateDto> patchDoc)
		{
			if (patchDoc == null) return this.BadRequest();
			if (!this.studentService.StudentExists(id)) return this.NotFound();
			var student = this.studentService.GetStudent(id);

			var studentForUpdateDto = Mapper.Map<StudentForUpdateDto>(student);

			patchDoc.ApplyTo(studentForUpdateDto, this.ModelState);

			if (studentForUpdateDto.Name == studentForUpdateDto.Surname)
				this.ModelState.AddModelError(nameof(StudentForUpdateDto), "The provided surname should be different from the name.");

			_ = this.TryValidateModel(studentForUpdateDto);
			if (!this.ModelState.IsValid) return new UnprocessableEntityObjectResult(this.ModelState);

			_ = Mapper.Map(studentForUpdateDto, student);
			if (!this.studentService.UpdateStudent(student)) throw new Exception($"Patching student {id} failed on save.");

			return this.NoContent();
		}
	}

	public partial class PeopleController
	{
		private string CreateHref(Guid id, string routeName, string fields = "") =>
			this.urlHelper.Link(routeName, new { id, fields });

		private IEnumerable<ILinkDto> CreateLinksStudent(Guid id, string fields = null) => new List<ILinkDto>
		{
			string.IsNullOrWhiteSpace(fields)
				? this.CreateHref(id, RouteName.GetStudent)
					.AddRelAndMethod(Rel.Self, Method.Get)
				: this.CreateHref(id, RouteName.GetStudent, fields)
					.AddRelAndMethod(Rel.Self, Method.Get),
			this.CreateHref(id, RouteName.CreateStudent)
				.AddRelAndMethod(Rel.CreateStudent, Method.Post),
			this.CreateHref(id, RouteName.PartiallyUpdateStudent)
				.AddRelAndMethod(Rel.PatchStudent, Method.Patch),
			this.CreateHref(id, RouteName.DeleteStudent)
				.AddRelAndMethod(Rel.DeleteStudent, Method.Delete),
		};
	}
}