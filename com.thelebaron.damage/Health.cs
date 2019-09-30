using Unity.Entities;

namespace thelebaron.Damage
{
    /// <summary>
    /// Basic component that stores limited information about health.
    /// </summary>
    public struct Health : IComponentData
    {
        public int  Value;
        public int  Max;
        public bool Invulnerable;
        public bool Gibbed;
        public int  LastDamageTaken;
        
        public static Health Default => new Health() {Value = 50, Max = 50};

        public void ApplyDamage(DamageEvent damageEvent)
        {
            if (Value <= 0 || Invulnerable)
                return;

            Value -= damageEvent.Amount;
            LastDamageTaken = damageEvent.Amount;
        }

        public void ApplyHealth(int amount)
        {
            if (amount <= 0)
                return;

            Value += amount;
        }
    }
}