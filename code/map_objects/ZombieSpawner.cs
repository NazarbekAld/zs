using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombieSpawner : Component
{

	[Property] public GameObject ZombiePrefab { get; set; }
	// [Property] public GameObject ZombieContainer { get; set; }

	// public float GetRandom() => Random.Shared.Float(1, 100);

	[Property]
	public int zlimit = 10;

	[Property]
	public float frequency = 20;

	TimeSince since = 0f;

	private TimeSince current = 0f;



	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

		Gizmo.Hitbox.BBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );
	}

	void SpawnZombie()
	{
		var zombie = ZombiePrefab.Clone( this.Transform.World );
		// zombie.Parent = ZombieContainer;
		zombie.Name = "Zombie";
		zombie.Tags.Add("Zombie_Gen");
		zombie.NetworkSpawn(Connection.Host);
	}

	public bool doSpawn() {
		if (current >= frequency) {
			return true;
		}
		return false;
	}

	protected override void OnFixedUpdate()
	{	
		frequency = 20 - (10 * (Connection.All.Count/16)) - (10 * since/50);
		if (doSpawn()) {
			current = 0f;
			int amount = 0;
			GameObject.Parent.Children.ForEach( (ob) => {
				if (ob.Tags.Has("Zombie_Gen")) {
					amount++;
				}
			} );
			if (amount < zlimit) {
				SpawnZombie();
			}
		}
	}

}
