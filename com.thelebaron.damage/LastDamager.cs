using Unity.Entities;

namespace thelebaron.Damage
{
    /// <summary>
    /// Contains an Entity reference to the last Entity that damaged this Actor/Entity
    /// </summary>
    public struct LastDamager : IComponentData
    {
        public Entity Value;
    }
}