﻿using System.Collections.Generic;
using SpaTemplate.Core.FacultyContext;

namespace SpaTemplate.Infrastructure
{
	public static class SeedData
	{
		public static void PopulateTestData(AppDbContext dbContext)
		{
            dbContext.People.RemoveRange(dbContext.People);
			dbContext.SaveChanges();
			for (var i = 0; i < 20; i++) AddStudent(dbContext, $"Name{i}", $"Surname{i}", i);
			dbContext.SaveChanges();
		}

		private static List<Course> SeedCourses()
		{
			var list = new List<Course>();
			for (var i = 0; i < 20; i++)
				list.Add(new Course
				{
					Title = $"Title{i}",
					Description = $"Description{i}"
				});

			return list;
		}

		private static void AddStudent(AppDbContext dbContext, string name, string surname, int age)
		{
			dbContext.People.Add(new Student
			{
				Name = name,
				Surname = surname,
				Age = age,
				Courses = SeedCourses()
			});
		}
	}
}