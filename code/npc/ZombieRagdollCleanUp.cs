using Sandbox;

public sealed class ZombieRagdollCleanUp : Component
{

	[Property]
	[Description("Clean up after N second(s)...")]
	public float cleanAfter = 20f;


	// [Description("Disable physics after N second(s)...")]
	// public float disablePhysicsAfter = 3f;
	
	[Property]
	ModelPhysics modelPhysics;

	private float expire;
	private float current;
	private bool cleaned = false;
	private bool physicsDisabled = false;
	protected override void OnStart()
	{
		
		expire = Time.Delta * 50 * cleanAfter;

	}

	protected override void OnUpdate()
	{
		if (cleaned) return;

		current += Time.Delta;
		// if (current >= disablePhysicsAfter) {
		// 	if (!physicsDisabled) {
		// 		if (modelPhysics != null) {
		//			Bro the NPE errors 💀💀💀💀💀
		// 			modelPhysics.PhysicsGroup.Remove();
		// 			modelPhysics.Destroy();
		// 		}
		// 		physicsDisabled = true;
		// 	}
		// }
		if (current >= expire) {
			cleaned = true;
			GameObject.Networked = false;
			GameObject.Destroy();
		}
	}


}