﻿using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Revolution.NetCode.LLAPI;
using GameHost.Simulation.Application;
using GameHost.Simulation.Utility.Resource;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.FreeRoam;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Network.Snapshots;
using PataNext.Module.Simulation.Network.Snapshots.Abilities;
using PataNext.Module.Simulation.Network.Snapshots.Resources;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

[assembly: RegisterAvailableModule("PataNext Simulation", "guerro", typeof(CustomModule))]

namespace PataNext.Module.Simulation
{
	public class CustomModule : GameHostModule
	{
		private void InjectFreeRoamSystems(SimulationApplication app)
		{
			app.Data.Collection.GetOrCreate(typeof(FreeRoamUnitProvider));
			app.Data.Collection.GetOrCreate(typeof(FreeRoamCharacterMovementSystem));
		}

		private void InjectSerializers(SimulationApplication app, SerializerCollection sc)
		{
			var appCtx = app.Data.Context;

			registerSnapshots();
			registerDescription();
			registerAuthority();

			void registerSnapshots()
			{
				resources();
				input();
				gameMode();
				gamePlay();

				void resources()
				{
					sc.Register(instigator => new GameGraphicResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new EquipmentResourceSnapshot.Serializer(instigator, appCtx));
					
					sc.Register(instigator => new UnitArchetypeResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new UnitKitResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new UnitAttachmentResourceSnapshot.Serializer(instigator, appCtx));
					
					sc.Register(instigator => new RhythmCommandResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new RhythmCommandIdentifierSnapshot.Serializer(instigator, appCtx));
				}

				void input()
				{
					sc.Register(instigator => new GameRhythmInputSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new FreeRoamInputSnapshot.Serializer(instigator, appCtx));
				}

				void gameMode()
				{

				}

				void gamePlay()
				{
					ability();
					unit();
					rhythmEngine();

					void ability()
					{
						sc.Register(instigator => new OwnerActiveAbilitySnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityEngineSetSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityActivationSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityCommandsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityModifyStatsOnChainingSnapshot.Serializer(instigator, appCtx));
					}

					void unit()
					{
						sc.Register(instigator => new UnitStatisticSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitPlayStateSnapshot.Serializer(instigator, appCtx));
						
						sc.Register(instigator => new UnitArchetypeSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitCurrentKitSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitDisplayedEquipmentSnapshot.Serializer(instigator, appCtx));
						
						sc.Register(instigator => new UnitDirectionSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitTargetOffsetSnapshot.Serializer(instigator, appCtx));
						
						sc.Register(instigator => new UnitControllerStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitFreeRoamMovementSnapshot.Serializer(instigator, appCtx));
						
						sc.Register(instigator => new GroundStateSnapshot.Serializer(instigator, appCtx));
					}
					
					void rhythmEngine()
					{
						sc.Register(instigator => new GameComboStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new GameComboSettingsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new GameCommandStateSnapshot.Serializer(instigator, appCtx));
						
						sc.Register(instigator => new RhythmEngineCommandProgressBufferSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEnginePredictedCommandBufferSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmCommandActionBufferSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new RhythmEngineControllerSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineExecutingCommandSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineLocalStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineSettingsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmSummonEnergySnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmSummonEnergyMaxSnapshot.Serializer(instigator, appCtx));
					}
				}
			}

			void registerDescription()
			{
				sc.Register(instigator => new IEntityDescription.Serializer<RhythmEngineDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<UnitTargetDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<UnitDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<MountDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<AbilityDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<ProjectileDescription>(instigator, appCtx));
				
				sc.Register(instigator => new Relative<RhythmEngineDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<UnitTargetDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<UnitDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<MountDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<AbilityDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<ProjectileDescription>.Serializer(instigator, appCtx));
				
				sc.Register(instigator => new OwnedRelative<AbilityDescription>.Serializer(instigator, appCtx));
			}

			void registerAuthority()
			{
				
			}
		}

		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						var ctx = simulationApplication.Data.Context;
						ctx.BindExisting(DefaultEntity<GameResourceDb<EquipmentResource>.Defaults>.Create(simulationApplication.Data.World, new()));
						ctx.BindExisting(DefaultEntity<GameResourceDb<GameGraphicResource>.Defaults>.Create(simulationApplication.Data.World, new()));
						ctx.BindExisting(DefaultEntity<GameResourceDb<RhythmCommandResource>.Defaults>.Create(simulationApplication.Data.World, new()));
						ctx.BindExisting(DefaultEntity<GameResourceDb<UnitArchetypeResource>.Defaults>.Create(simulationApplication.Data.World, new()));
						ctx.BindExisting(DefaultEntity<GameResourceDb<UnitAttachmentResource>.Defaults>.Create(simulationApplication.Data.World, new()));
						ctx.BindExisting(DefaultEntity<GameResourceDb<UnitKitResource>.Defaults>.Create(simulationApplication.Data.World, new()));

						simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IRhythmEngineSimulationPass.RegisterPass(),
							new[] {typeof(IUpdateSimulationPass.RegisterPass)},
							new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

						simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilityPreSimulationPass.RegisterPass(),
							new[] {typeof(IUpdateSimulationPass.RegisterPass), typeof(IRhythmEngineSimulationPass.RegisterPass)},
							new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

						simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilitySimulationPass.RegisterPass(),
							new[] {typeof(IUpdateSimulationPass.RegisterPass), typeof(IAbilityPreSimulationPass.RegisterPass)},
							new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

						simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.DontSerializeAbilityEngineSet));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.LocalRhythmCommandResourceManager));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Providers.PlayableUnitProvider));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.AbilityCollectionSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.UnitDescription.RegisterContainer));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.AbilityDescription.RegisterContainer));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.MountDescription.RegisterContainer));

						InjectFreeRoamSystems(simulationApplication);

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCalculatePlayStateSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.UpdateActiveAbilitySystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.ApplyAbilityStatisticOnChainingSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.ExecuteActiveAbilitySystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Team.UpdateTeamMovableAreaSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitPhysicsSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.AbilityControlVelocitySystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCalculateSeekingStateSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCollisionSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitPhysicsAfterBlockUpdateSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ManageComponentTagSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ProcessEngineSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.OnInputForRhythmEngine));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.GetNextCommandEngineSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ApplyCommandEngineSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.RhythmEngineResizeCommandBufferSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.BasicTestGameModeSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.InBasement.AtCityGameModeSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.StartYaridaTrainingGameMode));

						simulationApplication.Data.Collection.GetOrCreate(typeof(Network.MasterServer.Services.CreateGameSaveRequest.Process));
						simulationApplication.Data.Collection.GetOrCreate(typeof(Network.MasterServer.Services.ListGameSaveRequest.Process));


						// TEMPORARY SYSTEMS, THEY'LL NEED TO GET CONVERTED INTO REAL SYSTEMS.
						simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.TemporaryWayToGenerateDamageSystem));

						var serializerCollection = simulationApplication.Data.Collection.GetOrCreate(wc => new SerializerCollection(wc));
						InjectSerializers(simulationApplication, serializerCollection);
					}, default);
				}
			}
		}
	}
}