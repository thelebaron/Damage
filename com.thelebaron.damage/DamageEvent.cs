using Unity.Entities;

namespace thelebaron.Damage
{
    //[Serializable]
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