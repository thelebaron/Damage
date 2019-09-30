using Unity.Entities;

namespace thelebaron.Damage
{
    //Tag - if an entity has this it cant be updated by certain systems
    public struct Dead : IComponentData
    {
        public byte ZeroValue;
    }

}