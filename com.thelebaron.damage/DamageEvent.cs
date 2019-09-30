using Unity.Entities;

namespace thelebaron.Damage
{
    /// <summary>
    /// A damage event is a component that gets created with its own entity so that multiple instances 
    /// may be handled in a single frame by a damage stack.
    /// Useage: 
    /// Create new entity, 
    /// Create DamageEvent component 
    /// Attach to created entity.
    /// </summary>
    public struct DamageEvent: IComponentData
    {
        public int Amount;
        public Entity Receiver;
        public Entity Sender;
        //public float Time;
        //public Vector3 Direction;
        //public HitResult Hit;
    }
}

// damage event as free floating entity?
// no need for a buffer?
// less entity setup?