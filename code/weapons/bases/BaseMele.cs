using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneralGame;


public class BaseMele : WeaponComponent, IUse
{
	[Property] public float SeccondaryDamage { get; set; } = 50f;
	[Property] public float SeccondaryFireRate { get; set; } = 1f;
	[Property] public ParticleSystem ImpactEffect { get; set; }
	[Property] public SoundEvent AttackSound { get; set; }
	[Property] public SoundEvent HitSound { get; set; }
	[Property] public SoundEvent HitWorldSound { get; set; }
	[Property] public SoundEvent KillSound { get; set; }
	[Property] public AmmoType ammoType {get;set;}
	[Property] public float Range { get; set; }
	[Property] public float Punch { get; set; }
	[Property] public float staminaDrain { get; set; }

	[Property, Group("Mod")] public List<CustomBehaviour> behaviours;


	/*var ent = new BaseItem
	{
		Position = EyePosition + EyeRotation.Forward * 50,
		Rotation = EyeRotation

	};

	ent.Velocity = EyeRotation.Forward* 500;*/
	[Broadcast]
	public virtual void OnUse( Guid pickerId )
	{
		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = picker.Components.GetInDescendantsOrSelf<PlayerController>();
		if ( !player.IsValid() ) return;

		if ( player.IsProxy )
			return;

		if ( !player.Weapons.Has( GameObject ) )
		{
			this.owner = player;
			player.Weapons.Give( GameObject, false );
			GameObject.Destroy();
		}
	}

	public override void primaryAction()
	{
		attackTrace( true );
	}
	public override void seccondaryAction()
	{
		attackTrace( false );
	}


	void attackAnimation(bool primary)
	{
		if (ViewModel != null) {
			if (primary) {
				ViewModel.animation.attack();
			} else  {
				ViewModel.animation.secondary_attack();
			}
		}
	}

	public void doSound(SoundEvent e) {
		Sound.Play(e, owner.Transform.Position);
	}

	public virtual void attackTrace( bool primary )
	{
		if ( !NextAttackTime ) return;
		if (owner.stamina - staminaDrain <= 0) return;
		owner.stamina -= staminaDrain;
		base.primaryAction();

		
		var startPos = owner.PlyCamera.Transform.Position;
		var direction = owner.PlyCamera.Transform.Rotation.Forward;

		var endPos = startPos + direction * Range;
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.UsePhysicsWorld()
			.UseHitboxes()
			.WithTag(owner.Side == Side.HUMAN ? "hdmg" : "zdmg")
			.Run();
		
		var damage = Damage;

		

		doSound(AttackSound);

		IHealthComponent damageable = null;

		var hitev = new HitEvent();
		hitev.trace = trace;
		hitev.isPrimary = primary;
		if (behaviours != null)
			behaviours.ForEach((bh) => bh.hit(hitev));

		if ( trace.Component.IsValid() ) {
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
		}

		if ( damageable is not null )
		{
			if (!hitev.cancel.Contains("damage")) {
				if (damageable.Health - damage < 0)
				{
					// trace.Component.Velocity = direction * 500;
					doSound(KillSound);
				}
				else
				{
					doSound(HitSound);
				}
				damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id );
				owner.points += damage * 0.12;
			} else {
				hitev.cancel.Remove("damage");
			}
		}
		else if ( trace.Hit )
		{
			if ( ImpactEffect is null ) return;

			var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
			p.SetControlPoint( 0, trace.EndPosition );
			p.SetControlPoint( 0, Rotation.LookAt( trace.Normal ) );
			p.PlayUntilFinished( Task );

			doSound(HitWorldSound);
		}

		NextAttackTime = 1f / (primary ? FireRate : SeccondaryFireRate);
		

		if (!hitev.cancel.Contains("animation")) {
			attackAnimation( primary );
		} else {
			hitev.cancel.Remove("animation");
		}
		
		if (hitev.cancel.Count > 0) {
			Log.Warning("Found unused cancel queries! Unused: ");
			hitev.cancel.ForEach(Log.Warning);
			Log.Warning("-----END-----");
		}
	}
}
