using Unity.Entities;
using Unity.Jobs;

namespace thelebaron.Damage
{
    /// <summary>
    /// All this system does is add a DamageStack buffer to an entity with a health component. 
    /// It does nothing else(should this be rolled into the health system?).
    /// </summary>
    public class BootstrapDamageStackSystem : JobComponentSystem
    {
        private EntityQuery m_HealthQuery;
        private EndSimulationEntityCommandBufferSystem m_EndSimSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_HealthQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<Health>()
                },

                None = new ComponentType[]
                {
                    typeof(DamageStack)
                }
            });
        }

        [ExcludeComponent(typeof(DamageStack))]
        struct AddDamageStackJob : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Health c0)
            {
                EntityCommandBuffer.AddBuffer<DamageStack>(index, entity);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new AddDamageStackJob
            {
                EntityCommandBuffer = m_EndSimSystem.CreateCommandBuffer().ToConcurrent()
            };
            var handle = job.Schedule(this, inputDeps);
            
            m_EndSimSystem.AddJobHandleForProducer(handle);
            
            return handle;
        }
    }
}