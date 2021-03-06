﻿using System;
using System.Collections.Generic;
using System.Reflection;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using Module = PataNext.CoreAbilities.Server.Module;

[assembly: RegisterAvailableModule("PataNext Standard Abilities (Server Script)", "guerro", typeof(Module))]

namespace PataNext.CoreAbilities.Server
{
	public class Test : AppSystem
	{
		public Test(WorldCollection collection) : base(collection)
		{
		}
	}
	
	public class Module : GameHostModule
	{
		private List<Type> systemTypes = new List<Type>();
		
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			AppSystemResolver.ResolveFor<SimulationApplication>(typeof(Module).Assembly, systemTypes);

			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						foreach (var type in systemTypes)
							AddDisposable((IDisposable) simulationApplication.Data.Collection.GetOrCreate(type));
					}, default);
				}
			}
		}

		protected override void OnDispose()
		{
			systemTypes.Clear();
		}
	}
}