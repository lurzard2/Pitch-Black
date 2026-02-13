using DevInterface;
using JetBrains.Annotations;

namespace PitchBlack.Objects.RotPuff;

public class ManagedRotPuff : Pom.Pom.ManagedObjectType
{
	/// <summary>
	/// WARNING: base.ctor registers enum of value $name
	/// if you use this, don't register type of value ( = new(whatever, true) )
	/// </summary>
	public ManagedRotPuff() : base("RotPuff", "Pitch-Black", typeof(RotPuff), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation))
	{
	}

	/// <summary>
	/// RW makes APO before UAD, but POM wants UAD.
	/// We override this method to deliver UAD in vanilla compliant method
	/// </summary>
	/// <param name="placedObject"></param>
	/// <param name="room"></param>
	/// <returns></returns>
	[CanBeNull]
	public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
	{
		int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
		if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index,
			    pobjIndex) == false)
		{
			RotPuffAbstract rotPuffAbstract = new(room.world, null, room.GetWorldCoordinate(placedObject.pos),
				room.game.GetNewID(), pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
				{
					isConsumed = false
				};
			room.abstractRoom.AddEntity(rotPuffAbstract);
			rotPuffAbstract.placedObjectOrigin =
				room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
			//later on room would load and call ShortCutsReady
			//which would call realization for all APO
			//and add them to UAD
			//so we do regular procedures and tell POM to NOT do it for us
			return null;
		}
		return null;
	}
}