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
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using SpaTemplate.Core.FacultyContext;
	using SpaTemplate.Core.SharedKernel;
	using Xeinaemm.AspNetCore;

	/// <summary>
	///
	/// </summary>
	[Route(Route.StudentCollectionsApi)]
	[ValidateModel]
	[ApiController]
	public class PeopleCollectionsController : Controller
	{
		private readonly IStudentService studentService;
		private readonly IMapper mapper;

		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleCollectionsController"/> class.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="mapper"></param>
		public PeopleCollectionsController(IStudentService repository, IMapper mapper)
		{
			this.studentService = repository;
			this.mapper = mapper;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="studentForCreationDtos"></param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesDefaultResponseType]
		public IActionResult CreateStudentCollection(
			[FromBody] IEnumerable<StudentForCreationDto> studentForCreationDtos)
		{
			if (studentForCreationDtos == null) return this.BadRequest();
			var people = this.mapper.Map<IEnumerable<Student>>(studentForCreationDtos);

			foreach (var student in people)
				this.studentService.AddStudent(student);

			var peopleDto = this.mapper.Map<IEnumerable<StudentDto>>(people);
			return this.CreatedAtRoute(
				RouteName.GetStudentCollection,
				new
				{
					ids = string.Join(",", peopleDto.Select(a => a.Id)),
				}, peopleDto);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[HttpGet("({ids})", Name = RouteName.GetStudentCollection)]
		[ProducesDefaultResponseType]
		public IActionResult GetStudentCollection(
			[ModelBinder(BinderType = typeof(ArrayModelBinder))]
			IEnumerable<Guid> ids)
		{
			if (ids == null) return this.BadRequest();

			var collection = ids.ToList();
			var entities = this.studentService.GetCollection(collection);

			return collection.Count != entities.Count ? this.NotFound() : (IActionResult)this.Ok(this.mapper.Map<IEnumerable<StudentDto>>(entities));
		}
	}
}