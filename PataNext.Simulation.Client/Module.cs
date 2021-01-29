﻿using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Systems.GhRpc;
using PataNext.Simulation.Client.Systems;
using PataNext.Simulation.Client.Systems.Inputs;

namespace PataNext.Simulation.Client
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			global.Collection.GetOrCreate(typeof(ConnectToServerRpc));
			global.Collection.GetOrCreate(typeof(DisconnectFromServerRpc));
			global.Collection.GetOrCreate(typeof(SendServerNoticeRpc));

			foreach (var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					if (!simulationApplication.AssignedEntity.Has<IClientSimulationApplication>())
						continue;

					simulationApplication.Schedule(() =>
					{
						simulationApplication.Data.Collection.GetOrCreate(typeof(RegisterRhythmEngineInputSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(RegisterFreeRoamInputSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(AbilityHeroVoiceManager));

						simulationApplication.Data.Collection.GetOrCreate(typeof(InterpolateForeignUnitsPosition));
					}, default);
				}
			}
		}
	}
}