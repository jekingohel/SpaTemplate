using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Infrastructure.Core;

namespace SpaTemplate.Web.Core
{
    [Route(Route.StudentCollectionsApi)]
    public class PeopleCollectionsController : Controller
    {
        private readonly IStudentService _studentService;

        public PeopleCollectionsController(IStudentService repository) =>
            _studentService = repository;

        [HttpGet("({ids})", Name = RouteName.GetStudentCollection)]
        public IActionResult GetStudentCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            IEnumerable<Guid> ids)
        {
            if (ids == null) return BadRequest();

            var collection = ids.ToList();
            var entities = _studentService.GetCollection(collection);

            if (collection.Count != entities.Count) return NotFound();
            return Ok(Mapper.Map<IEnumerable<StudentDto>>(entities));
        }

        [HttpPost]
        public IActionResult CreateStudentCollection(
            [FromBody] IEnumerable<StudentForCreationDto> studentForCreationDtos)
        {
            if (studentForCreationDtos == null) return BadRequest();
            var people = Mapper.Map<IEnumerable<Student>>(studentForCreationDtos);

            foreach (var student in people)
                _studentService.AddStudent(student);

            var peopleDto = Mapper.Map<IEnumerable<StudentDto>>(people);
            return CreatedAtRoute(RouteName.GetStudentCollection,
                new
                {
                    ids = string.Join(",", peopleDto.Select(a => a.Id))
                }, peopleDto);
        }
    }
}