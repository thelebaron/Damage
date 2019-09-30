using Unity.Mathematics;

namespace thelebaron.Damage
{
    public struct HitResult
    {
        //public Actor Actor;
        public float3 ImpactPoint;
        public float3 ImpactNormal;
        public float3 Distance;
        public float3 Direction;

    }
}