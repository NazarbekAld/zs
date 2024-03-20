using System;
using GeneralGame;
using Sandbox;

public sealed class PropBase : Component, IHealthComponent, IProp, IUse
{
	public LifeState LifeState {get; private set;} = LifeState.Alive;

	[Property]
	[Sync]
	public float MaxHealth {get; set;} = 100f;

	[Property]
	[Sync]
	public float Health {get; set;} = 100f;

	[Sync]
	public float damages {get;set;} = 0f;

	[Property]
	[Sync]
	public Guid? owner {get; set;} = null;

	[Property]
	[Sync]
	public int nails {get; set;} = 0;

	[Property]
	[Sync]
	public float repairable {get; set;} = 0f;

	public float repairableMax {get;set;}

	[Property]
	public float nailStrength {get;set;} = 100f;
	[Property]
	public PrefabScene propinfo;	

	private Rigidbody physics { get; set;} = null;

	protected override void OnEnabled()
	{
		repairableMax = nailStrength * 4;
	}
	protected override void OnAwake()
	{
		physics = Components.GetInAncestorsOrSelf<Rigidbody>();
	}

	public void OnUse( Guid pickerId )
	{
		RevokePrevOwner();
		var pick = Scene.Children.Find((ob) => ob.Id == pickerId);

		if ( pick is null ) return;

		var controller = pick.Components.GetInAncestorsOrSelf<PlayerController>();

		if ( controller == null ) return;

		controller.pickedProp = this;
		controller.pickedPropDis = Transform.Position.Distance(pick.Transform.Position);
		owner = pickerId;


		Tags.Add( "picked" );
		lockPhysics();
	}

	public void RevokePrevOwner()
	{
		if ( owner is not null )
		{
			var prev = Scene.Children.Find( ( ob ) => ob.Id == owner );
			var prevc = prev.Components.GetInAncestorsOrSelf<PlayerController>();
			if (prev is null) return;
			if (prevc is null) return;
			ReleaseProp();
			prevc.pickedProp = null;
		}
	}

	[Broadcast]
	public void ReleaseProp( )
	{
		if (physics.IsValid)
		{
			physics.Gravity = true;
		}
		GameObject.Tags.Remove( "picked" );
	}

	[Broadcast]
	public void lockPhysics()
	{
		var physics = Components.GetInAncestorsOrSelf<Rigidbody>();
		physics.Gravity = false;

	}

	[Broadcast]
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{

		if (IsProxy) return;

		if (type == DamageType.Zombie) {
			Health -= damage;
		}//
		if (type == DamageType.Bullet) {
			//Health -= damage;
		}
		damages += damage;
		if (Health <= 0) {
			GameObject.Destroy();
		}
	}

	[Broadcast]
	public void GoTo(Vector3 location)
	{
		Transform.Position = location;
	}
	[Broadcast]
	public void GoTo( Vector3 location, Rotation rot )
	{
		Transform.Position = location;
		Transform.Rotation = rot; 
	}
	
	[Broadcast]
	public void repair(float amount) {

		if (amount + Health > MaxHealth) {
			amount = MaxHealth - Health;
		}
		if (amount > repairable) {
			amount = repairable;
		}

		repairable-=amount;
		Health += amount;
	}

	[Broadcast]
	public void nail() {
		var l = new PhysicsLock();
		l.X = true;
		l.Y = true;
		l.Z = true;
		l.Pitch = true;
		l.Yaw = true;
		l.Roll = true;
		if (IsProxy) {
			physics.Locking = l;
			GameObject.Tags.Remove( "picked" );

			GameObject.Tags.Add( "nailed" );
			return;
		}
		physics.Locking = l;
		repairable += nailStrength;
    	nails += 1;


		if (damages <= 0) {
			Health = MaxHealth;
		}


		GameObject.Tags.Remove( "picked" );
		GameObject.Tags.Add( "nailed" );
	}

	[Broadcast]
	public void spawnPropInfo(Vector3 positon) {
		var inf = propinfo.Clone();
		inf.Transform.Position = positon;
		inf.SetParent(GameObject);
		inf.NetworkMode = NetworkMode.Object;

		var infc = inf.Components.GetInChildrenOrSelf<PropInfo>();
		infc.setSource(this);
	}

}
