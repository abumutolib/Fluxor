using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	/// <summary>
	/// An options class for configuring Fluxor
	/// </summary>
	public class Options
	{
		internal AssemblyScanSettings[] AssembliesToScan { get; private set; } = new AssemblyScanSettings[0];
		internal Type[] MiddlewareTypes = new Type[0];

		private delegate void RegisterServiceByTypeHandler(Type serviceType);
		private RegisterServiceByTypeHandler RegisterServiceByType;

		private delegate void RegisterServiceWithImplementationTypeHandler(Type serviceType, Type implementationType);
		private RegisterServiceWithImplementationTypeHandler RegisterServiceWithImplementationType;

		private delegate void RegisterServiceUsingFactoryHandler(
			Type serviceType,
			Func<IServiceProvider, object> implementationFactory);
		private RegisterServiceUsingFactoryHandler RegisterServiceUsingFactory;

		/// <summary>
		/// Service collection for registering services
		/// </summary>
		public readonly IServiceCollection Services;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="services"></param>
		public Options(IServiceCollection services)
		{
			Services = services;
			UseScopedDependencyInjection();
		}

		/// <summary>
		/// Enables automatic discovery of features/effects/reducers
		/// </summary>
		/// <param name="additionalAssembliesToScan">A collection of assemblies to scan</param>
		/// <returns>Options</returns>
		public Options ScanAssemblies(
			Assembly assemblyToScan,
			params Assembly[] additionalAssembliesToScan)
		{
			if (assemblyToScan == null)
				throw new ArgumentNullException(nameof(assemblyToScan));

			var allAssemblies = new List<Assembly> { assemblyToScan };
			if (additionalAssembliesToScan != null)
				allAssemblies.AddRange(additionalAssembliesToScan);

			var newAssembliesToScan = allAssemblies.Select(x => new AssemblyScanSettings(x)).ToList();
			newAssembliesToScan.AddRange(AssembliesToScan);
			AssembliesToScan = newAssembliesToScan.ToArray();

			return this;
		}

		/// <summary>
		/// Registers a service for dependency injection
		/// </summary>
		/// <param name="serviceType">The dependency type that will be injected</param>
		public void RegisterService(Type serviceType)
		{
			RegisterServiceByType(serviceType);
		}

		/// <summary>
		/// Registers a service for dependency injection
		/// </summary>
		/// <param name="serviceType">The dependency type that will be injected</param>
		/// <param name="implementationType">The class type to instantiate</param>
		public void RegisterService(Type serviceType, Type implementationType)
		{
			RegisterServiceWithImplementationType(serviceType, implementationType);
		}

		/// <summary>
		/// Registers a service for dependency injection
		/// </summary>
		/// <param name="serviceType">The dependency type that will be injected</param>
		/// <param name="implementationFactory">A factory method to create the injected dependency</param>
		public void RegisterService(
					Type serviceType,
					Func<IServiceProvider, object> implementationFactory)
		{
			RegisterServiceUsingFactory(serviceType, implementationFactory);
		}

		/// <summary>
		/// Enables the developer to specify a class that implements <see cref="IMiddleware"/>
		/// which should be injected into the <see cref="IStore.AddMiddleware(IMiddleware)"/> method
		/// after dependency injection has completed.
		/// </summary>
		/// <typeparam name="TMiddleware">The Middleware type</typeparam>
		/// <returns>Options</returns>
		public Options AddMiddleware<TMiddleware>()
			where TMiddleware : IMiddleware
		{
			if (Array.IndexOf(MiddlewareTypes, typeof(TMiddleware)) > -1)
				return this;

			RegisterService(typeof(TMiddleware));
			Assembly assembly = typeof(TMiddleware).Assembly;
			string @namespace = typeof(TMiddleware).Namespace;

			AssembliesToScan = new List<AssemblyScanSettings>(AssembliesToScan)
			{
				new AssemblyScanSettings(assembly, @namespace)
			}
			.ToArray();

			MiddlewareTypes = new List<Type>(MiddlewareTypes)
			{
				typeof(TMiddleware)
			}
			.ToArray();
			return this;
		}

		/// <summary>
		/// Registers discovered services using a Scoped container
		/// </summary>
		public void UseScopedDependencyInjection()
		{
			RegisterServiceByType = (serviceType) => Services.AddScoped(serviceType);
			RegisterServiceWithImplementationType = (serviceType, implementingType) => Services.AddScoped(serviceType, implementingType);
			RegisterServiceUsingFactory = (serviceType, factory) => Services.AddScoped(serviceType, factory);
		}

		/// <summary>
		/// Registers discovered services using a Singleton container
		/// </summary>
		public void UseSingletonDependencyInjection()
		{
			RegisterServiceByType = (serviceType) => Services.AddSingleton(serviceType);
			RegisterServiceWithImplementationType = (serviceType, implementingType) => Services.AddSingleton(serviceType, implementingType);
			RegisterServiceUsingFactory = (serviceType, factory) => Services.AddSingleton(serviceType, factory);
		}

	}
}
