using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Infrastructure;
using SpaTemplate.Infrastructure.Core;

namespace SpaTemplate.Web.Core
{
    [Route(Route.PeopleApi)]
    [ValidateModel]
    public partial class PeopleController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IUrlHelper _urlHelper;

        public PeopleController(
            IStudentService studentService, IUrlHelper urlHelper)
        {
            _studentService = studentService;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = RouteName.GetPeople)]
        public IActionResult GetPeople(StudentParameters parameters,
            [FromHeader(Name = Header.Accept)] string mediaType)
        {
            if (!_studentService.StudentMappingExists(parameters)) 
                return BadRequest();
            if (!_studentService.StudentPropertiesExists(parameters)) 
                return BadRequest();

            var people = _studentService.GetPagedList(parameters);
            var studentDtos = Mapper.Map<List<StudentDto>>(people);

            if (mediaType == MediaType.OutputFormatterJson)
            {
                Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(people.CreateBasePagination()));
                var values = studentDtos.ShapeDataCollectionWithHateoasLinks(parameters.Fields, CreateLinksStudent);
                var links = _urlHelper.CreateLinks(RouteName.GetPeople, parameters, people);
                return Ok(HateoasDto.CreateHateoasDto(values, links));
            }

            Response.Headers.Add(Header.XPagination, JsonConvert.SerializeObject(
                _urlHelper.CreateHateoasPagination(RouteName.GetPeople, people, parameters)));

            return Ok(studentDtos.ShapeDataCollection(parameters.Fields));
        }

        [HttpGet("{id}", Name = RouteName.GetStudent)]
        public IActionResult GetStudent(Guid id, StudentParameters parameters,
            [FromHeader(Name = Header.Accept)] string mediaType)
        {
            if (!_studentService.StudentPropertiesExists(parameters)) return BadRequest();

            var student = _studentService.GetStudent(id);
            if (student == null) return NotFound();

            return mediaType == MediaType.OutputFormatterJson
                ? Ok(student.ShapeDataWithoutParameters<StudentDto, Student>(CreateLinksStudent))
                : Ok(Mapper.Map<StudentDto>(student));
        }

        [HttpPost(Name = RouteName.CreateStudent)]
        [RequestHeaderMatchesMediaType(Header.ContentType, new[] {MediaType.InputFormatterJson})]
        public IActionResult CreateStudent([FromBody] StudentForCreationDto studentForCreationDto)
        {
            if (studentForCreationDto == null) return BadRequest();

            var student = Mapper.Map<Student>(studentForCreationDto);
            if (!_studentService.AddStudent(student)) throw new Exception("Creating an Student failed on save.");
            return CreatedAtRoute(RouteName.GetStudent, new {id = student.Id},
                student.ShapeDataWithoutParameters<StudentDto, Student>(CreateLinksStudent));
        }

        [HttpPatch("{id}", Name = RouteName.PartiallyUpdateStudent)]
        public IActionResult PartiallyUpdateStudent(Guid id,
            [FromBody] JsonPatchDocument<StudentForUpdateDto> patchDoc)
        {
            if (patchDoc == null) return BadRequest();
            if (!_studentService.StudentExists(id)) return NotFound();
            var student = _studentService.GetStudent(id);

            var studentForUpdateDto = Mapper.Map<StudentForUpdateDto>(student);

            patchDoc.ApplyTo(studentForUpdateDto, ModelState);

            if (studentForUpdateDto.Name == studentForUpdateDto.Surname)
                ModelState.AddModelError(nameof(StudentForUpdateDto),
                    "The provided surname should be different from the name.");

            TryValidateModel(studentForUpdateDto);
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            Mapper.Map(studentForUpdateDto, student);
            if (!_studentService.UpdateStudent(student)) throw new Exception($"Patching student {id} failed on save.");

            return NoContent();
        }


        [HttpDelete("{id}", Name = RouteName.DeleteStudent)]
        public IActionResult DeleteStudent(Guid id)
        {
            var student = _studentService.GetStudent(id);
            if (student == null) return NotFound();

            if (!_studentService.DeleteStudent(student)) throw new Exception($"Deleting Student {id} failed on save.");
            return NoContent();
        }
    }

    public partial class PeopleController
    {
        private string CreateHref(Guid id, string routeName, string fields = "") =>
            _urlHelper.Link(routeName, new {id, fields});

        private IEnumerable<ILinkDto> CreateLinksStudent(Guid id, string fields = null) => new List<ILinkDto>
        {
            string.IsNullOrWhiteSpace(fields)
                ? CreateHref(id, RouteName.GetStudent)
                    .AddRelAndMethod(Rel.Self, Method.Get)
                : CreateHref(id, RouteName.GetStudent, fields)
                    .AddRelAndMethod(Rel.Self, Method.Get),
            CreateHref(id, RouteName.CreateStudent)
                .AddRelAndMethod(Rel.CreateStudent, Method.Post),
            CreateHref(id, RouteName.PartiallyUpdateStudent)
                .AddRelAndMethod(Rel.PatchStudent, Method.Patch),
            CreateHref(id, RouteName.DeleteStudent)
                .AddRelAndMethod(Rel.DeleteStudent, Method.Delete)
        };
    }
}