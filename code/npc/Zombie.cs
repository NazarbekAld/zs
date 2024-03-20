using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

public sealed class Zombie : Component, IHealthComponent
{
	[Property] public GameObject body { get; set; }
	[Property] public GameObject eye { get; set; }
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public SoundEvent hitSounds { get; set; }
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; private set; } = 100f;
	[Property] public GameObject ZombieRagedol { get; set; }

	[Sync] public Vector3 wish { get; set; }
	[Sync] public Vector3 velocity { get; set; }

	private NavMeshAgent agent;
    private GameObject target { get; set; }
	public TimeSince timeSinceHit = 0;

	public List<Guid> ignore = new();

	// Ignore target if this didnt do any damage or stuck
	private TimeSince timeout { get; set; } = 0f;

	[Property]
	public GameObject eyeObject { get; set; }


	protected override void OnAwake()
	{
		if ( IsProxy ) return;
		agent = Components.GetInAncestorsOrSelf<NavMeshAgent>();
	}
	protected override void OnFixedUpdate()
	{

		if ( LifeState == LifeState.Dead ) return;
		animationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;

		UpdateAnimtions();

		if ( IsProxy ) return;

		target = null;
		float minDistance = float.MaxValue;
		Scene.Children.ForEach( ( ob ) =>
		{
			if ( !ob.Active ) return;
			if (ignore.Contains(ob.Id)) return; 

			if ( ob.Tags.Has( "zdmg" ) )
			{
				var dis = Vector3.DistanceBetween( ob.Transform.Position, GameObject.Transform.Position );
				if ( dis < minDistance )
				{
					minDistance = dis;
					target = ob;
				}
			}
		} );
		// Bros have no purpose 💀💀💀
		if (target == null)
		{
			LifeState = LifeState.Dead;
			var zombie = ZombieRagedol.Clone( this.GameObject.Transform.Position, this.GameObject.Transform.Rotation );
			zombie.NetworkSpawn();
			this.GameObject.Destroy();
			return;
		}
		var targett = target.Transform.Position;
	
		if ( Vector3.DistanceBetween( Vector3.Zero, agent.Velocity ) < 50f )
		{
			if (timeout >= 40)
			{
				ignore.Add( target.Id );
				timeout = 0;
			}
		}

		if ( targett.Distance(Transform.Position) < 70f)
		{
			agent.Stop();
			wish = agent.WishVelocity;
			velocity = agent.Velocity;
			NormalTrace();
		}
		else
		{
			agent.MoveTo( target.Transform.Position );
			wish = agent.WishVelocity;
			velocity = agent.Velocity;
		}

	}

	void UpdateAnimtions()
	{
		animationHelper.WithWishVelocity(wish);
		animationHelper.WithVelocity(velocity);
		
		// Nav mesh agent will do the job.!
		//var target = Scene.Children.Find((ob) => ob.Id == targetId);
		//if ( target is null ) return;
		//var targetRot = Rotation.LookAt(target.Transform.Position.WithZ(Transform.Position.z) - body.Transform.Position);
		//body.Transform.Rotation = Rotation.Slerp(body.Transform.Rotation, targetRot, Time.Delta * 5.0f);
		//animationHelper.DuckLevel = isCrouching ? 1f : 0f;


		animationHelper.Enabled = true;
	}

	[Broadcast]
	void HitAnimation()
	{
		animationHelper.Target.Set( "b_attack", true );
		Sound.Play( hitSounds, Transform.Position );

	}

	void NormalTrace()
	{

		if (GameObject is not null) {
			if ( target.Transform.Position.Distance( Transform.Position ) < 70f && timeSinceHit > 1.4)
			{
				IHealthComponent damageable;
				damageable = target.Components.GetInAncestorsOrSelf<IHealthComponent>();
				if (damageable.LifeState == LifeState.Dead) return; // Nah
				damageable.TakeDamage( DamageType.Zombie, 15, 1, 1, GameObject.Id );
				HitAnimation();
				timeout = 0f;
				timeSinceHit = 0;

			}
		}
	}

	[Broadcast]
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( type == DamageType.Bullet )
		{
			var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, position );
			p.SetControlPoint( 0, Rotation.LookAt( force.Normal * -1f ) );
			p.PlayUntilFinished( Task );
		}

		if ( IsProxy )
			return;


		Health = MathF.Max( Health - damage, 0f );

		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			var zombie = ZombieRagedol.Clone( this.GameObject.Transform.Position, this.GameObject.Transform.Rotation );
			zombie.NetworkSpawn();
			GameObject.NetworkMode = NetworkMode.Object;
			this.GameObject.Destroy();
		}


	}

}
