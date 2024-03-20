using Sandbox;

public sealed class NailHandler : Component
{
	[Property]
	GameObject attachedProp;

	protected override void OnFixedUpdate()
	{
		// if (attachedProp != null) {
		// 	if (!attachedProp.IsValid) {
		// 		GameObject.Destroy();
		// 	}
		// }
	}
}