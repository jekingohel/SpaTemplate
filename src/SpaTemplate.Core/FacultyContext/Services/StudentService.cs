// -----------------------------------------------------------------------
// <copyright file="StudentService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using SpaTemplate.Core.SharedKernel;

	public class StudentService : IHandle<StudentCompletedEvent>, IStudentService
	{
		private readonly IPropertyMappingService propertyMappingService;
		private readonly IRepository repository;
		private readonly ITypeHelperService typeHelperService;

		public StudentService(
			IRepository repository,
			ITypeHelperService typeHelperService,
			IPropertyMappingService propertyMappingService)
		{
			this.repository = repository;
			this.typeHelperService = typeHelperService;
			this.propertyMappingService = propertyMappingService;
		}

		public bool AddStudent(Student student) => this.repository.AddEntity(student);

		public bool DeleteStudent(Student student) => this.repository.DeleteEntity(student);

		// TODO: Create specification
		public List<Student> GetCollection(IEnumerable<Guid> ids) =>
			this.repository.GetCollection<Student>().Where(a => ids.Contains(a.Id)).ToList();

		public PagedList<Student> GetPagedList<TParameters>(TParameters parameters)
			where TParameters : IParameters => this.repository.GetCollection<Student, StudentDto>(
			new StudentParametersSpecification(parameters), parameters);

		public Student GetStudent(Guid studentId) => this.repository.GetEntity<Student>(studentId);

		public void Handle(StudentCompletedEvent domainEvent)
		{
			if (domainEvent == null)
				throw new NullReferenceException($"Collection {nameof(domainEvent)} cannot be null");

			// Do Nothing
		}

		public bool StudentExists(Guid studentId) => this.repository.ExistsEntity<Student>(studentId);

		public bool StudentMappingExists<TParameters>(TParameters parameters)
			where TParameters : IParameters
			=>
			this.propertyMappingService.ValidMappingExistsFor<StudentDto, Student>(
				parameters.OrderBy);

		public bool StudentPropertiesExists<TParameters>(TParameters parameters)
			where TParameters : IParameters
			=>
			this.typeHelperService.TypeHasProperties<StudentDto>(parameters.Fields);

		public bool UpdateStudent(Student student) => this.repository.UpdateEntity(student);
	}
}