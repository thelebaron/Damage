using Unity.Mathematics;

namespace Damage
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