// -----------------------------------------------------------------------
// <copyright file="QuartzScheduler.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Web.Quartz
{
	using System.Threading.Tasks;
	using global::Quartz;
	using Xeinaemm.Quartz;

	public static class QuartzScheduler
	{
		public static JobKey ExceptionsJobKey => new JobKey("Exceptions", "Xeinaemm.Quartz");

		public static JobKey QuickJobJobKey => new JobKey("Quick", "Xeinaemm.Quartz");

		public static JobKey CoreJobKey => new JobKey("NetCore", "Xeinaemm.Quartz");

		public static async Task RegisterScheduledJobsAsync()
		{
			await QuartzHelper.ScheduleJobAsync<ExceptionsJob>(new QuartzJob
			{
				JobKey = ExceptionsJobKey,
				CronExpression = "1 * * * * ?",
			}).ConfigureAwait(false);

			await QuartzHelper.ScheduleJobAsync<QuickJob>(new QuartzJob
			{
				JobKey = QuickJobJobKey,
				CronExpression = "1 * * * * ?",
			}).ConfigureAwait(false);

			await QuartzHelper.ScheduleJobAsync<CoreJob>(new QuartzJob
			{
				JobKey = CoreJobKey,
				CronExpression = "1 * * * * ?",
			}).ConfigureAwait(false);
		}
	}
}