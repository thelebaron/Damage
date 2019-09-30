using Unity.Entities;

namespace Damage
{
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

            if (Value <= 0)
            {
                var gibThresh = Max * -1;

                if (Value <= gibThresh)
                    Gibbed = true;
            }
        }

        public void ApplyHealth(int amount)
        {
            if (amount <= 0)
                return;

            Value += amount;
        }
    }
}