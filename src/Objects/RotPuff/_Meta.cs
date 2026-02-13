using Fisobs.Core;

namespace PitchBlack.Objects.RotPuff;

public static class _Meta
{
	public static void Apply()
	{
		Pom.Pom.RegisterManagedObject(new ManagedRotPuff());
		Content.Register(new RotPuffFisob());
	}
}