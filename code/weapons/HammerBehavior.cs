using System;
using Sandbox;

namespace GeneralGame {

    public class HammerBehaviour: CustomBehaviour {
        
        [Property]
        public float distance = 100f;

        [Property]
        public BaseMele caller;

        [Property]
        public PrefabScene nail;
        [Property]
        public PrefabScene info;

        [Property]
        public float repairRate {get; set;} = 15f;

		public override void hit( HitEvent e )
		{
            var pc = caller.owner;

            if (pc.pickedProp == null) {

                var hit = e.trace;
                if (hit.Hit && hit.GameObject.Tags.Has("nailed")) {
                    e.cancel.Add("damage");
                }

                if (hit.Hit && !e.isPrimary && hit.GameObject.Tags.Has("nailed")) {
                    var ob = hit.GameObject;
                    var prop = ob.Components.GetInAncestorsOrSelf<PropBase>();
                    if (prop.nails >= 4) {
                        return;
                    }
                    if (!pc.Ammo.CanTake(AmmoType.Nail, 1, out var _)) {
                        return;
                    }
                    pc.Ammo.TryTake(AmmoType.Nail, 1, out var _);

                    e.cancel.Add("animation");
                    caller.ViewModel.animation.attack();
                    prop.nail();

                    var sp = pc.PlyCamera.Transform.Position + Vector3.Zero;
                    var d = pc.PlyCamera.Transform.Rotation.Forward + Vector3.Zero;
                    var rot = pc.PlyCamera.Transform.LocalRotation + Rotation.Identity;

                    var ep = sp + d * distance;

                    shootNail(prop, sp, ep, rot, pc);
                    caller.owner.points += 5;
                }

                if (hit.Hit && e.isPrimary && hit.GameObject.Tags.Has("nailed")) {
                    var ob = hit.GameObject;
                    var prop = ob.Components.GetInAncestorsOrSelf<PropBase>();
                    var repair = repairRate;

                    prop.repair(repair);
                    caller.owner.points += repair * 0.2;
                    caller.doSound(caller.HitSound);
                }
                return;
            }
            // do not
            e.cancel.Add("damage");

            var startPos = pc.PlyCamera.Transform.Position;
		    var direction = pc.PlyCamera.Transform.Rotation.Forward;

            var endPos = startPos + direction * distance;

            var ground = Scene.Trace.Ray(startPos, endPos)
                    .UseHitboxes()
                    .WithoutTags("picked", "player")
                    .Size(5f)
                    .IgnoreGameObject(pc.GameObject)
                .Run();
            if (ground.Hit && !e.isPrimary) {
                if (!pc.Ammo.CanTake(AmmoType.Nail, 1, out var _)) {
                    return;
                }
                pc.Ammo.TryTake(AmmoType.Nail, 1, out var _);
                
                var backupProp = pc.pickedProp;

                if (pc.pickedProp.nails >= 4) {
                    Log.Info("Max nail exeeded. (4)");
                    return;
                }

                pc.pickedProp.nail();
                pc.pickedProp = null;

                e.cancel.Add("animation");

                

                var sp = pc.PlyCamera.Transform.Position + Vector3.Zero;
		        var d = pc.PlyCamera.Transform.Rotation.Forward + Vector3.Zero;
                var rot = pc.PlyCamera.Transform.LocalRotation + Rotation.Identity;

                var ep = sp + d * distance;
                
                caller.ViewModel.animation.attack();

                var nailloc = shootNail(backupProp, sp, ep, rot, pc);
                caller.owner.points += 5;
                backupProp.spawnPropInfo(nailloc.EndPosition);
                
            }

            
		}

    

        public SceneTraceResult shootNail(PropBase backupProp, Vector3 sp, Vector3 ep, Rotation rot, PlayerController pc) {
            var nailloc = Scene.Trace.Ray(sp, ep)
                    .WithoutTags("picked", "player")
                    .UseHitboxes()
                    .HitTriggers()
                    .IgnoreGameObject(pc.GameObject)
                    .Size(5f)
                    .Run();
                if (nail != null) {
                    if (nailloc.Hit) {

                        var nailob = nail.Clone();
                        caller.doSound(caller.HitSound);
                        nailob.Transform.Position = nailloc.EndPosition;
                        nailob.Transform.LocalRotation = rot;
                        nailob.Transform.Scale += 1;
                        nailob.SetParent(backupProp.GameObject);
                        nailob.NetworkSpawn();
                        
                    }
                } else {
                    Log.Warning("No nail prefab");
                }
            return nailloc;
        }
        
	}


}