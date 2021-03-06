﻿using System;
using Collections.Pooled;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(OnInputForRhythmEngine))]
	public class GetNextCommandEngineSystem : RhythmEngineSystemBase
	{
		public GetNextCommandEngineSystem(WorldCollection collection) : base(collection)
		{
		}

		private PooledList<GameResource<RhythmCommandResource>> cmdOutput = new PooledList<GameResource<RhythmCommandResource>>();
		private EntityQuery                                     entityQuery;

		public override void OnRhythmEngineSimulationPass()
		{
			var commandSetBuffer = new FixedBuffer128<GameEntityHandle>();
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmCommandResource>(),
				GameWorld.AsComponentType<RhythmCommandActionBuffer>()
			}))
			{
				if (commandSetBuffer.GetCapacity() > commandSetBuffer.GetLength())
				{
					commandSetBuffer.Add(entity);
				}
				else
					Console.WriteLine("couldn't add more commands!");
			}
			
			if (commandSetBuffer.GetLength() == 0)
				return;

			foreach (var entity in entityQuery ??= CreateEntityQuery(new []
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineExecutingCommand>(),
				GameWorld.AsComponentType<RhythmEngineCommandProgressBuffer>(),
				GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>(),
				
				GameWorld.AsComponentType<SimulationAuthority>(),
			}))
			{
				ref readonly var state    = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var settings = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				if (!state.CanRunCommands)
					continue;

				var commandProgression = GameWorld.GetBuffer<RhythmEngineCommandProgressBuffer>(entity);
				var predictedCommands  = GameWorld.GetBuffer<RhythmEnginePredictedCommandBuffer>(entity);

				ref var executingCommand = ref GameWorld.GetComponentData<RhythmEngineExecutingCommand>(entity);

				var output = cmdOutput;
				output.Clear();

				RhythmCommandUtility.GetCommand(GameWorld, commandSetBuffer.Span, commandProgression.Reinterpret<FlowPressure>(), output, false, settings.BeatInterval);

				predictedCommands.Clear();
				predictedCommands.Reinterpret<GameResource<RhythmCommandResource>>().AddRange(output.Span);
				if (predictedCommands.Count == 0)
				{
					RhythmCommandUtility.GetCommand(GameWorld, commandSetBuffer.Span, commandProgression.Reinterpret<FlowPressure>(), output, true, settings.BeatInterval);
					if (output.Count > 0)
					{
						predictedCommands.Reinterpret<GameResource<RhythmCommandResource>>().AddRange(output.Span);
					}

					continue;
				}

				// this is so laggy clients don't have a weird things when their command has been on another beat on the server
				var targetBeat            = commandProgression[^1].Value.FlowBeat + 1;

				executingCommand.Previous            = executingCommand.CommandTarget;
				executingCommand.CommandTarget       = output[0];
				executingCommand.ActivationBeatStart = targetBeat;
				
				ref var identifier   = ref GameWorld.GetComponentData<RhythmCommandIdentifier>(executingCommand.CommandTarget.Handle);
				var     beatDuration = identifier.Duration;
				
				executingCommand.ActivationBeatEnd = targetBeat + beatDuration;
				executingCommand.WaitingForApply   = true;

				var power = 0.0f;
				for (var i = 0; i != commandProgression.Count; i++)
				{
					// perfect
					if (commandProgression[i].Value.GetAbsoluteScore() <= FlowPressure.Perfect)
						power += 1.0f;
				}

				executingCommand.Power = power / commandProgression.Count;
				commandProgression.Clear();
			}
		}
	}
}