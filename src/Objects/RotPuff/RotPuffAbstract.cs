namespace PitchBlack.Objects.RotPuff;

public class RotPuffAbstract : AbstractConsumable
{
	public RotPuffAbstract(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int pObjIndex, PlacedObject.ConsumableObjectData data = null) : base(world, Enums.AbstractObjectType.RotPuff, realizedObject, pos, ID, pos.room, pObjIndex, data)
	{
	}
	
	public override void Realize()
	{
		realizedObject ??= new Objects.RotPuff.RotPuff(this, world);
		base.Realize();
	}
}