// -----------------------------------------------------------------------
// <copyright file="DomainEventDispatcher.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Autofac;
	using SpaTemplate.Core.SharedKernel;

	public class DomainEventDispatcher : IDomainEventDispatcher
	{
		private readonly IComponentContext container;

		public DomainEventDispatcher(IContainer container) => this.container = container;

		public DomainEventDispatcher(IComponentContext container) => this.container = container;

		public void Dispatch(BaseDomainEvent domainEvent)
		{
			var handlerType = typeof(IHandle<>).MakeGenericType(domainEvent.GetType());
			var wrapperType = typeof(DomainEventHandler<>).MakeGenericType(domainEvent.GetType());
			var handlers = (IEnumerable)this.container.Resolve(typeof(IEnumerable<>).MakeGenericType(handlerType));
			var wrappedHandlers = handlers.Cast<object>()
				.Select(handler => (DomainEventHandler)Activator.CreateInstance(wrapperType, handler));

			foreach (var handler in wrappedHandlers) handler.Handle(domainEvent);
		}

		private abstract class DomainEventHandler
		{
			public abstract void Handle(BaseDomainEvent domainEvent);
		}

		private class DomainEventHandler<T> : DomainEventHandler
			where T : BaseDomainEvent
		{
			private readonly IHandle<T> handler;

			public DomainEventHandler(IHandle<T> handler) => this.handler = handler;

			public override void Handle(BaseDomainEvent domainEvent) => this.handler.Handle((T)domainEvent);
		}
	}
}