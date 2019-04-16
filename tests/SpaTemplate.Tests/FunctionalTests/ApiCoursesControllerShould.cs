// -----------------------------------------------------------------------
// <copyright file="ApiCoursesControllerShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.FunctionalTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.JsonPatch;
	using Newtonsoft.Json;
	using SpaTemplate.Core.FacultyContext;
	using SpaTemplate.Core.SharedKernel;
	using SpaTemplate.Infrastructure.Api;
	using SpaTemplate.Tests.Helpers;
	using Xunit;

	/// <inheritdoc />
	/// <summary>
	/// <para>
	///     A - Standard calls to API without combinations(POST + GET, POST + DELETE etc.)
	///     B - Extended calls that test API in isolation without prepared data in the database to remove hardcoded values.
	///         Firstly we call POST to prepare object and then exercise another action(ie. DELETE).
	///     C - If you exercise more actions (ie. 3) then use next letter to distinct number of calls, ie. C = 3 D = 4, E = 5 etc.
	///     Number defines tests for particular action(ie. GET)
	///     1 - GET,
	///     2 - POST,
	///     3 - PATCH,
	///     4 - PUT,
	///     5 - DELETE.
	/// </para>
	/// <para>    The functional test needs to be tested in particular order due to domino's effect of API, ie. if POST will fail, others too.</para>
	/// </summary>
	public partial class ApiCoursesControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		private readonly HttpClient client;

		public ApiCoursesControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			this.client = factory.CreateClientWithDefaultRequestHeaders();

		[Theory]
		[InlineData("?orderBy=unknown")]
		[InlineData("?fields=dummy")]
		public async Task A1_GetReturnsBadRequest_FieldAndMappingNotExistAsync(string field)
		{
			var get = await this.GetCoursesAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), field)
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task A2_PostReturnsNotFound_PostCourseToStudentThatNotExistsAsync(CourseForCreationDto dto)
		{
			var post = await this.PostAsync(Guid.NewGuid(), dto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task A4_PutReturnsCreated_NewCourseAsync(CourseForUpdateDto dto)
		{
			var put = await this.PutAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid(), dto)
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.Created, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task A4_PutReturnsNotFound_StudentNotExistsAsync(CourseForUpdateDto dto)
		{
			var put = await this.PutAsync(Guid.NewGuid(), Guid.NewGuid(), dto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Post, Rel.CreateCourseForStudent, 1)]
		[InlineData(Method.Patch, Rel.PartiallyUpdateCourse, 2)]
		[InlineData(Method.Put, Rel.UpdateCourse, 3)]
		[InlineData(Method.Delete, Rel.DeleteCourse, 4)]
		public async Task B1_GetReturnsCollection_WithHateoasInsideLinksAsync(string method, string rel, int number)
		{
			var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

			Assert.Equal(10, get.Values.Count);
			Assert.Equal(5, get.Values[0].Links.Count);

			Assert.Equal(method, get.Values[0].Links[number].Method);
			Assert.Equal(rel, get.Values[0].Links[number].Rel);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Get, Rel.NextPage, 1)]
		public async Task B1_GetReturnsCollection_WithHateoasOutsideLinksAsync(string method, string rel, int number)
		{
			var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

			Assert.Equal(2, get.Links.Count);
			Assert.Equal(method, get.Links[number].Method);
			Assert.Equal(rel, get.Links[number].Rel);
		}

		[Theory]
		[AutoMoqData]
		public async Task B2_PostReturnsBadRequest_UnprocessableEntityAsync(CourseForCreationDto courseForCreationDto)
		{
			courseForCreationDto.Description = "Dummy";
			courseForCreationDto.Title = "Dummy";
			var (post, _) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task B2_PostReturnsCreatedAsync(CourseForCreationDto dto)
		{
			var (post, _) = await this.PostCourseAsync(dto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C1_GetPaginationNextPageNotNullAsync(CourseForCreationDto dto)
		{
			var (_, studentId) = await this.PostCourseAsync(dto).ConfigureAwait(false);
			var get = await this.GetCoursesAsync(studentId).ConfigureAwait(false);
			var header = get.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.NextPage);
		}

		[Theory]
		[AutoMoqData]
		public async Task C2_PostReturnsSameEntity_CourseCreatedAsync(CourseForCreationDto courseForCreationDto)
		{
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
			var get = JsonConvert.DeserializeObject<CourseDto>(
				await (await this.GetCourseAsync(studentId, post.Id).ConfigureAwait(false))
					.Content.ReadAsStringAsync().ConfigureAwait(false));

			Assert.Equal(post.Id, get.Id);
		}

		[Theory]
		[AutoMoqData]
		public async Task C3_PatchReturnsBadRequest_PatchDocIsNullAsync(CourseForCreationDto courseForCreationDto)
		{
			var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
			var patch = await this.PatchAsync(studentId, Guid.NewGuid(), null).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.BadRequest, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C3_PatchReturnsCreated_NewCourseAsync(CourseForCreationDto courseForCreationDto)
		{
			var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			_ = patchDoc.Replace(x => x.Description, "Dummy");

			var patch = await this.PatchAsync(studentId, Guid.NewGuid(), patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.Created, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C3_PatchReturnsNoContent_ValidPatchAsync(CourseForCreationDto courseForCreationDto)
		{
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			_ = patchDoc.Replace(x => x.Description, "Dummy");

			var patch = await this.PatchAsync(studentId, post.Id, patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C3_PatchReturnsUnprocessableEntity_PropertiesEqualExistingCourseAsync(
			CourseForCreationDto courseForCreationDto)
		{
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);

			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			_ = patchDoc.Replace(x => x.Description, "Dummy");
			_ = patchDoc.Replace(x => x.Title, "Dummy");

			var patch = await this.PatchAsync(studentId, post.Id, patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C3_PatchReturnsUnprocessableEntity_PropertiesEqualsNewCourseAsync(
			CourseForCreationDto courseForCreationDto)
		{
			var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			_ = patchDoc.Replace(x => x.Description, "Dummy");
			_ = patchDoc.Replace(x => x.Title, "Dummy");

			var patch = await this.PatchAsync(studentId, Guid.NewGuid(), patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C4_PutReturnsNoContent_ExistingCourseAsync(CourseForUpdateDto courseForCreationDto)
		{
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
			var put = await this.PutAsync(studentId, post.Id, courseForCreationDto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C4_PutReturnsUnprocessableEntity_PropertiesEqualAsync(CourseForUpdateDto courseForCreationDto)
		{
			courseForCreationDto.Description = "Dummy";
			courseForCreationDto.Title = "Dummy";
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
			var put = await this.PutAsync(studentId, post.Id, courseForCreationDto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C5_DeleteReturnsNoContent_CourseDeletedAsync(CourseForCreationDto courseForCreationDto)
		{
			var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
			var delete = await this.DeleteAsync(studentId, post.Id).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
		}

		[Fact]
		public async Task A1_GetReturnsNotFound_StudentNotExistsAsync()
		{
			var get = await this.GetCoursesAsync(Guid.NewGuid()).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Fact]
		public async Task A2_PostReturnsBadRequest_CourseIsNullAsync()
		{
			var (post, _) = await this.PostCourseAsync(null).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
		}

		[Fact]
		public async Task A3_PatchReturnsNotFound_StudentNotExistsAsync()
		{
			var patch = await this.PatchAsync(Guid.NewGuid(), Guid.NewGuid(), new JsonPatchDocument<CourseForUpdateDto>())
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
		}

		[Fact]
		public async Task A5_DeleteReturnsNotFound_StudentNotExistsAsync()
		{
			var delete = await this.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Fact]
		public async Task B1_GetReturnsCollection_WithoutHateoasAsync()
		{
			var (courses, _) = await this.GetIEnumerableCoursesAndStudentIdAsync().ConfigureAwait(false);
			Assert.Equal(10, courses.Count());
		}

		[Fact]
		public async Task B1_GetReturnsNotFound_CourseNotExistAsync()
		{
			var get = await this.GetCourseAsync(
				await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false),
				Guid.NewGuid())
				.ConfigureAwait(false);

			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Fact]
		public async Task B1_GetReturnsNotNullHref_CoursesWithHateoasAsync()
		{
			var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

			Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
			Assert.All(get.Links, x => Assert.NotNull(x.Href));
		}

		[Fact]
		public async Task B1_GetReturnsOK_GetCoursesAsync()
		{
			var get = await this.GetCoursesAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false))
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}

		[Fact]
		public async Task B1_GetReturnsOK_GetCoursesWithHateoasAsync()
		{
			var getCourse = await this.GetCoursesAsync(await this.GetFirstHateoasStudentIdAsync().ConfigureAwait(false))
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
		}

		[Fact]
		public async Task B4_PutReturnsBadRequest_CourseIsNullAsync()
		{
			var put = await this.PutAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid(), null)
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
		}

		[Fact]
		public async Task B5_DeleteReturnsNotFound_CourseNotExistsAsync()
		{
			var delete = await this.DeleteAsync(
				await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid())
				.ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Fact]
		public async Task C1_GetReturnsOK_CourseExistsInStudentAsync()
		{
			var (get, studentId) = await this.GetIEnumerableCoursesAndStudentIdAsync().ConfigureAwait(false);

			var getCourse = await this.GetCourseAsync(studentId, get.First().Id).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
		}

		[Fact]
		public async Task C1_GetReturnsOK_CourseShapedWithHateoasAsync()
		{
			var (get, studentId) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

			var getCourse = await this.GetCourseAsync(studentId, get.Values[0].Id).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
		}
	}

	public partial class ApiCoursesControllerShould
	{
		private Task<HttpResponseMessage> DeleteAsync(Guid studentId, Guid courseId) =>
			this.client.DeleteAsync(Api.Course.ToApiUrl(studentId, courseId));

		private Task<HttpResponseMessage> GetCourseAsync(Guid studentId, Guid courseId, string fields = "") =>
			this.client.GetAsync(Api.Course.ToApiUrl(studentId, courseId, fields));

		private Task<HttpResponseMessage> GetCoursesAsync(Guid studentId, string fields = "") =>
			this.client.GetAsync(Api.Courses.ToApiUrl(studentId, fields));

		private async Task<Guid> GetFirstHateoasStudentIdAsync()
		{
			_ = this.client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
			return (await this.GetPeopleAsync<HateoasDto<StudentValuesDto>>().ConfigureAwait(false)).Values[0].Id;
		}

		private async Task<Guid> GetFirstIEnumerableStudentIdAsync() =>
			(await this.GetPeopleAsync<IEnumerable<StudentDto>>().ConfigureAwait(false)).First().Id;

		private async Task<(HateoasDto<CourseValuesDto> Courses, Guid StudentId)> GetHateoasCoursesAndStudentIdAsync()
		{
			var studentId = await this.GetFirstHateoasStudentIdAsync().ConfigureAwait(false);
			return (JsonConvert.DeserializeObject<HateoasDto<CourseValuesDto>>(
				await (await this.GetCoursesAsync(studentId).ConfigureAwait(false))
					.Content.ReadAsStringAsync().ConfigureAwait(false)), studentId);
		}

		private async Task<(IEnumerable<CourseDto> Courses, Guid StudentId)> GetIEnumerableCoursesAndStudentIdAsync()
		{
			var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
			return (JsonConvert.DeserializeObject<IEnumerable<CourseDto>>(
				await (await this.GetCoursesAsync(studentId).ConfigureAwait(false))
					.Content
					.ReadAsStringAsync().ConfigureAwait(false)), studentId);
		}

		private async Task<TStudent> GetPeopleAsync<TStudent>() =>
			JsonConvert.DeserializeObject<TStudent>(await (await this.client?.GetAsync(Api.People))
				.Content.ReadAsStringAsync().ConfigureAwait(false));

		private Task<HttpResponseMessage> PatchAsync(
			Guid studentId,
			Guid courseId,
			JsonPatchDocument<CourseForUpdateDto> patchDoc) => this.client != null
			? this.client.PatchAsync(
				Api.Course.ToApiUrl(studentId, courseId),
				patchDoc.Content(MediaType.PatchFormatterJson))
			: Task.FromResult<HttpResponseMessage>(null);

		private Task<HttpResponseMessage> PostAsync(Guid studentId, CourseForManipulationDto dto) =>
			this.client != null
				? this.client.PostAsync(Api.Courses.ToApiUrl(studentId), dto.Content(MediaType.InputFormatterJson))
				: Task.FromResult<HttpResponseMessage>(null);

		private async Task<(HttpResponseMessage Response, Guid StudentId)> PostCourseAsync(
			CourseForManipulationDto courseDto)
		{
			var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
			return (await this.PostAsync(studentId, courseDto).ConfigureAwait(false), studentId);
		}

		private async Task<(T course, Guid StudentId)> PostCourseAsync<T>(
			CourseForManipulationDto courseDto)
		{
			var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
			return (JsonConvert.DeserializeObject<T>(await (await this.PostAsync(studentId, courseDto).ConfigureAwait(false))
					.Content
					.ReadAsStringAsync().ConfigureAwait(false)),
				studentId);
		}

		private Task<HttpResponseMessage> PutAsync(Guid studentId, Guid courseId, CourseForManipulationDto dto) =>
			this.client.PutAsync(Api.Course.ToApiUrl(studentId, courseId), dto.Content(MediaType.InputFormatterJson));

		private class CourseValuesDto : StudentDto
		{
			public List<LinkDto> Links { get; } = new List<LinkDto>();
		}

		private class HateoasDto<T>
		{
			public List<T> Values { get; } = new List<T>();

			public List<LinkDto> Links { get; } = new List<LinkDto>();
		}

		private class StudentValuesDto : StudentDto
		{
			public List<StudentDto> Values { get; } = new List<StudentDto>();
		}
	}
}