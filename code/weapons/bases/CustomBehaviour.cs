using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace GeneralGame {

    public class CustomBehaviour: Component {
        
        /**
            Call this if weapon fired. (even trace didnt even hit anything)
        **/
        public virtual void hit(HitEvent e) {}
    }

    // TODO
    public class CustomActionGraphBehaviour: CustomBehaviour {

        public override void hit(HitEvent e) {}

    }

    public class HitEvent {
        public SceneTraceResult trace {get; set;}
        public bool isPrimary {get; set;} = true;

        /**
            List of strings what to cancel.
            Example: 'damage' should cancel the damage.
            IMPORTANT: You should log the unused cancel queries.
        **/
        public List<string> cancel {get;} = new();
    }

}