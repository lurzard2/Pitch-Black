using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;

namespace PitchBlack.Objects.RotPuff;

public class RotPuffFisob : Fisob
{
	public RotPuffFisob() : base(Enums.AbstractObjectType.RotPuff)
	{
	}

	public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
	{
		return new RotPuffAbstract(world, null, entitySaveData.Pos, entitySaveData.ID, -1);
	}
}