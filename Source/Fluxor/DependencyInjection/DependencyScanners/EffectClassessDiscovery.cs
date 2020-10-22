using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectClassessDiscovery
	{
		internal static DiscoveredEffectClass[] DiscoverEffectClasses(
			Options options,
			IEnumerable<Type> allCandidateTypes)
		{
			DiscoveredEffectClass[] discoveredEffectInfos =
				allCandidateTypes
					.Where(t => typeof(IEffect).IsAssignableFrom(t))
					.Where(t => t != typeof(EffectWrapper<>))
					.Select(t => new DiscoveredEffectClass(implementingType: t))
					.ToArray();

			foreach (DiscoveredEffectClass discoveredEffectInfo in discoveredEffectInfos)
				options.RegisterService(discoveredEffectInfo.ImplementingType);

			return discoveredEffectInfos;
		}
	}
}
