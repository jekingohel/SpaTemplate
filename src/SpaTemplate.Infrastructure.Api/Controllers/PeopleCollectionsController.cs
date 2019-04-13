// -----------------------------------------------------------------------
// <copyright file="PeopleCollectionsController.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutoMapper;
	using Microsoft.AspNetCore.Mvc;
	using SpaTemplate.Core.FacultyContext;
	using SpaTemplate.Core.SharedKernel;

	[Route(Route.StudentCollectionsApi)]
	public class PeopleCollectionsController : Controller
	{
		private readonly IStudentService studentService;

		public PeopleCollectionsController(IStudentService repository) =>
			this.studentService = repository;

		[HttpPost]
		public IActionResult CreateStudentCollection(
			[FromBody] IEnumerable<StudentForCreationDto> studentForCreationDtos)
		{
			if (studentForCreationDtos == null) return this.BadRequest();
			var people = Mapper.Map<IEnumerable<Student>>(studentForCreationDtos);

			foreach (var student in people)
				this.studentService.AddStudent(student);

			var peopleDto = Mapper.Map<IEnumerable<StudentDto>>(people);
			return this.CreatedAtRoute(
				RouteName.GetStudentCollection,
				new
				{
					ids = string.Join(",", peopleDto.Select(a => a.Id)),
				}, peopleDto);
		}

		[HttpGet("({ids})", Name = RouteName.GetStudentCollection)]
		public IActionResult GetStudentCollection(
			[ModelBinder(BinderType = typeof(ArrayModelBinder))]
			IEnumerable<Guid> ids)
		{
			if (ids == null) return this.BadRequest();

			var collection = ids.ToList();
			var entities = this.studentService.GetCollection(collection);

			if (collection.Count != entities.Count) return this.NotFound();
			return this.Ok(Mapper.Map<IEnumerable<StudentDto>>(entities));
		}
	}
}