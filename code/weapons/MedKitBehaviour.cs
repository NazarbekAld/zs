using System;
using Sandbox;

namespace GeneralGame {

    public class MedKitBehaviour: CustomBehaviour {

        [Property]
        BaseMele medkit;

        [Property]
        SoundEvent errorSound;

        [Property]
        public float healPerShot = 16f;

        Connection connection;

		protected override void OnUpdate()
		{
			base.OnUpdate();
            if (connection is null)
                connection = Network.OwnerConnection;

		}
		public override void hit( HitEvent e )
		{
			
            e.cancel.Add("damage");

            var ammo = medkit.owner.Ammo.Get(AmmoType.MedSupplies);

            var chatmed = medkit.owner.PlyCamera.GameObject.Components.GetInParentOrSelf<Chat>();

            var heal = healPerShot;
            if (ammo < healPerShot) {
                heal = ammo;
            }
            if (ammo == 0) {
                errorSound.Volume = 5f;
                Sound.Play(errorSound);
                chatmed.AddTextLocal("Server", "Not enough medical supplies!");
                e.cancel.Add("animation");
                return;
            }

            if (!e.isPrimary) {
                
                if (medkit.owner.Health + heal > medkit.owner.MaxHealth) {
                    heal = medkit.owner.MaxHealth - medkit.owner.Health;
                } 

                medkit.owner.Heal(heal);
                
                medkit.owner.Ammo.TryTake(AmmoType.MedSupplies, MathX.FloorToInt(heal), out var _);

                chatmed.AddTextLocal("Server", "Healed yourself " + " for " + heal + " Medical Supplies.");

                return;  
            }

            var startPos = medkit.owner.PlyCamera.Transform.Position;
            var direction = medkit.owner.PlyCamera.Transform.Rotation.Forward;

            var endPos = startPos + direction * medkit.Range;
            var trace = Scene.Trace.Ray( startPos, endPos )
                .IgnoreGameObject( GameObject.Root )
                .HitTriggers()
                .UseHitboxes()
                .Size(10f)
                .Run();
            if (trace.Hit && trace.GameObject.Tags.HasAny("hheal")) {
                var another = trace.GameObject.Components.GetInAncestorsOrSelf<PlayerController>();
                if (another.Health + heal > another.MaxHealth) {
                    heal = another.MaxHealth - another.Health;
                }

                medkit.owner.Ammo.TryTake(AmmoType.MedSupplies, MathX.FloorToInt(heal), out var _);

                another.Heal(heal);
                
                var chat = another.PlyCamera.GameObject.Components.GetInChildrenOrSelf<Chat>(true);
                chat.AddTextLocal("Server", connection.DisplayName + " healed you " + heal + "hp.");
                chatmed.AddTextLocal("Server", "You healed " + trace.GameObject.Name + " for " + heal + " Medical Supplies.");
            } else {
                e.cancel.Add("animation");
                Sound.Play(errorSound);
            }

		}
	}

}