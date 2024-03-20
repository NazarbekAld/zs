using Sandbox;
using Sandbox.Citizen;
using System;
using System.Linq;
using System.Numerics;

namespace GeneralGame;

public abstract class WeaponComponent : Component
{
	[Property] public string DisplayName { get; set; }
	[Property] public float DeployTime { get; set; } = 0.5f;
	[Property] public float DamageForce { get; set; } = 5f;
	[Property] public float Damage { get; set; } = 10f;
	[Property] public float FireRate { get; set; } = 3f;
	[Property] public GameObject ViewModelPrefab { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public SoundEvent DeploySound { get; set; }
	[Property] public SoundEvent HolsterSound { get; set; }
	[Property] public bool IsDeployed { get; set; }
	[Property] public Vector3 idlePos { get; set; }
	[Property] public Vector3 aimPos { get; set; }
	[Property] public Rotation aimRotation { get; set; }
	[Property] public Rotation runRotation { get; set; }
	[Property] public float cost {get;set;} = 20f;
	[Property, DefaultValue(WeaponType.Others)] public WeaponType type { get; set; }
	public bool HasViewModel => ViewModel.IsValid();
	public PlayerController owner { get; set; }
	public SkinnedModelRenderer ModelRenderer { get; set; }
	public ViewModel ViewModel { get; set; }
	public TimeUntil NextAttackTime { get; set; }
	public SkinnedModelRenderer EffectRenderer => ViewModel.IsValid() ? ViewModel.ModelRenderer : ModelRenderer;

	protected override void OnStart()
	{
		if ( !owner.IsValid() ) return;
		if ( IsDeployed )
			OnDeployed();
		else
			OnHolstered();
		base.OnStart();
	}

	protected override void OnAwake()
	{
		ModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override void OnDestroy()
	{
		if ( IsDeployed )
		{
			OnHolstered();
			IsDeployed = false;
		}

		base.OnDestroy();
	}

	

	[Broadcast]
	public virtual void Deploy()
	{
		if ( !IsDeployed )
		{
			IsDeployed = true;
			
			OnDeployed();
		}
	}

	[Broadcast]
	public virtual void Holster()
	{
		if ( IsDeployed )
		{
			OnHolstered();
			IsDeployed = false;
		}
	}
	
	public virtual void primaryAction()
	{
		var player = Components.GetInAncestors<PlayerController>();

		if ( player.IsValid() )
		{
			foreach ( var animator in player.Animators )
			{
				animator.Target.Set( "b_attack", true );
			}
		}

	}
	public virtual void primaryActionRelease()
	{


	}

	public virtual void seccondaryAction()
	{
		
	}
	public virtual void seccondaryActionRelease()
	{

	}


	public virtual void reloadAction()
	{

	}
	

	protected virtual void OnDeployed()
	{
		var player = Components.GetInAncestors<PlayerController>();

		

		if ( player.IsValid() )
		{
			foreach ( var animator in player.Animators )
			{
				animator.TriggerDeploy();
			}
		}
		if (ViewModel != null)
			if (ViewModel.IsValid)
				ModelRenderer.Enabled = !HasViewModel;
		
		if ( DeploySound is not null )
		{
			Sound.Play( DeploySound, Transform.Position );
		}

		if ( !IsProxy )
		{
			CreateViewModel();
		}
		
		NextAttackTime = DeployTime;
	}

	protected virtual void OnHolstered()
	{
		if (ModelRenderer != null)
			if (ModelRenderer.IsValid) 
				ModelRenderer.Enabled = false;
		DestroyViewModel();
	}
	
	private void DestroyViewModel()
	{
		ViewModel?.GameObject?.Destroy();
		ViewModel = null;
	}

	private void CreateViewModel()
	{
		if ( !ViewModelPrefab.IsValid() )
			return;
		
		var player = Components.GetInAncestorsOrSelf<PlayerController>();
		var viewModelGameObject = ViewModelPrefab.Clone();
		viewModelGameObject.SetParent( player.ViewModelRoot, false );
		
		ViewModel = viewModelGameObject.Components.GetInChildrenOrSelf<ViewModel>();
		ViewModel.SetWeaponComponent( this );
		ViewModel.SetCamera( player.PlyCamera );
		
		if (ModelRenderer != null)
			ModelRenderer.Enabled = false;
	}

	public static string toRealisticAmmoType(AmmoType type) {
		var ammoType = type.ToString();
		if (type == AmmoType.Pistol) {
			ammoType = "9mm";
		}
		if (type == AmmoType.Rifle) {
			ammoType = "7.62x39mm";
		}
		if (type == AmmoType.MedSupplies) {
			ammoType = "Medical Supply";
		}
		return ammoType;
	}

	public static float boxAmount(AmmoType type) {
		float amount = 1;

		if (type == AmmoType.Pistol) {
			amount = 15f;
		}
		if (type == AmmoType.Rifle){
			amount = 30f;
		}
		if (type == AmmoType.MedSupplies) {
			amount = 50f;
		}

		return amount;
	}

	public static float priceTag(AmmoType type, float amount) {
		float price = 10;

		if (type == AmmoType.Pistol) {
			price = 0.5f;
		}
		if (type == AmmoType.Rifle) {
			price = 0.7f;
		}
		if (type == AmmoType.MedSupplies) {
			price = 0.5f;
		}

		return price * amount;
	}
}
