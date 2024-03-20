using Sandbox;

namespace GeneralGame {

    public abstract class ViewModelAnimStrat : Component 
    {
        public virtual void deploy() {}
        public virtual void attack() {}
        public virtual void secondary_attack() {}
        public virtual void aiming(bool toggle) {}
        public virtual void jump() {}
    }

	public class NativeModelAnimationStrat : ViewModelAnimStrat
	{

        [Property]
        public SkinnedModelRenderer mdl {get; set;} 

        [Property, DefaultValue("b_deploy")]
        public string deployAnimation {get; set;} = "b_deploy";

        [Property, DefaultValue("b_attack")]
        public string attackAnimation {get; set;} = "b_attack";

        [Property, DefaultValue("b_power_attack")]
        public string secondaryAttackAnimation {get; set;} = "b_power_attack";

        [Property, DefaultValue("b_aiming")]
        public string aimAnimation {get; set;} = "b_aiming";

        [Property, DefaultValue("b_jump")]
        public string jumpAnimation {get; set;} = "b_jump";

		public override void aiming(bool toggle)
		{
            mdl.Set(aimAnimation, toggle);
		}

		public override void attack()
		{
            mdl.Set(attackAnimation, true);
		}

		public override void deploy()
		{
			mdl.Set(deployAnimation, true);
		}

		public override void secondary_attack()
		{
			mdl.Set(secondaryAttackAnimation, true);
		}
	}

}