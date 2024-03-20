using Sandbox;

namespace GeneralGame {

    public class SimpleViewModelAnimationStrategy: ViewModelAnimStrat {
        [Property]
        ViewModel vm;

        private Rotation backup;
        [Property, Group("attack")]
        Rotation attackRot = Rotation.Identity;
        [Property, Group("attack")]
        float returnAfterAttack = 0.5f;

        [Property, Group("secondary_attack")]
        Rotation secondaryAttackRot = Rotation.Identity;
        [Property, Group("secondary_attack")]
        float returnAfterSecondaryAttack = 0.5f;

		protected override void OnEnabled()
		{
			if (IsProxy) return;
            backup = vm.shiftRot;
		}

		public async override void attack()
		{
            vm.shiftRot = attackRot;
            await Task.DelaySeconds(returnAfterAttack);
            vm.shiftRot = backup;
		}

		public async override void secondary_attack()
		{
			vm.shiftRot = secondaryAttackRot;
            await Task.DelaySeconds(returnAfterSecondaryAttack);
            vm.shiftRot = backup;
		}

    

	}

}