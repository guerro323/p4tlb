﻿using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Resources
{
	public struct UnitArchetypeResourceSnapshot : IReadWriteSnapshotData<UnitArchetypeResourceSnapshot>,
	                                              ISnapshotSyncWithComponent<UnitArchetypeResource>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitArchetypeResourceSnapshot, UnitArchetypeResource>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public CharBuffer64 Identifier;

		public void Serialize(in BitBuffer buffer, in UnitArchetypeResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddString(Identifier.ToString());
		}

		public void Deserialize(in BitBuffer buffer, in UnitArchetypeResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Identifier = CharBufferUtility.Create<CharBuffer64>(buffer.ReadString());
		}

		public void FromComponent(in UnitArchetypeResource component, in EmptySnapshotSetup setup)
		{
			Identifier = component.Value;
		}

		public void ToComponent(ref UnitArchetypeResource component, in EmptySnapshotSetup setup)
		{
			component = new UnitArchetypeResource(Identifier);
		}
	}
}