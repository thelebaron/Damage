
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace thelebaron.Damage
{
    [UpdateInGroup(typeof(DamageUpdateGroup))]
    public class HealthSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem m_EndSim;
        private EntityQuery                            m_DamageEventsQuery;
        private EntityQuery                            m_HistoryQuery;

        protected override void OnCreate()
        {
            m_EndSim            = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_DamageEventsQuery = GetEntityQuery(typeof(DamageEvent));
            m_HistoryQuery      = GetEntityQuery(typeof(DamageHistory));
        }

        [BurstCompile]
        private struct HistoryJob : IJobChunk
        {
            [ReadOnly] public float                    Time;
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<DamageEvent> DamageEvents;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public ArchetypeChunkBufferType<DamageHistory> DamageHistoryType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntity    = chunk.GetNativeArray(EntityType);
                var chunkHistories = chunk.GetBufferAccessor(DamageHistoryType);

                for (int index = 0; index < chunkEntity.Length; index++)
                {
                    var entity  = chunkEntity[index];
                    var history = chunkHistories[index];

                    for (var i = 0; i < DamageEvents.Length; i++)
                    {
                        if (entity.Equals(DamageEvents[i].Receiver))
                        {
                            var de = DamageEvents[i];

                            var dh = new DamageHistory
                            {
                                TimeOccured     = Time,
                                TookDamage      = true,
                                Damage          = de.Amount,
                                Instigator      = de.Sender,
                                LastDamageEvent = de
                            };

                            history.Add(dh);
                        }
                    }
                }
            }
        }

        
        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        [ExcludeComponent(typeof(Dead))]
        private struct AddDamageStackJob : IJobForEachWithEntity<DamageEvent>
        {
            public EntityCommandBuffer.Concurrent ECB;
            [NativeDisableParallelForRestriction] public BufferFromEntity<DamageStack> DamageStackBuffer;
            
            public void Execute(Entity entity, int index, ref DamageEvent c0)
            {
                if (DamageStackBuffer.Exists(c0.Receiver))
                {
                    DamageStackBuffer[c0.Receiver].Add(c0);
                }
                ECB.DestroyEntity(index, entity);
            }
        }

        /// <summary>
        /// Applies the damage to the health component. Todo: merge damage and apply in one go? so can be gibbed?
        /// </summary>
        [BurstCompile]
        private struct ProcessDamage : IJobForEachWithEntity_EBC<DamageStack, Health>
        {
            
            public void Execute(Entity entity, int index, DynamicBuffer<DamageStack> stacks, ref Health health)
            {
                var damagetotal = new DamageEvent();
                int damage = 0;
                // Apply damage
                for (int de = 0; de < stacks.Length; de++)
                {
                    damage += stacks[de].Value.Amount;
                    
                }
                stacks.Clear();
                damagetotal.Amount = damage;
                
                health.ApplyDamage(damagetotal);

            }
        }
        
        /// <summary>
        /// Add dead tag to health components that are technically dead
        /// </summary>
        //[BurstCompile]
        [ExcludeComponent(typeof(Dead))]
        private struct TagDeadJob : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public void Execute(Entity entity, int index,  ref Health health)
            {
                if(health.Value <= 0)
                    EntityCommandBuffer.AddComponent(index, entity, new Dead());
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var entityType        = GetArchetypeChunkEntityType();
            var damagehistoryType = GetArchetypeChunkBufferType<DamageHistory>();
            
            var historyJob = new HistoryJob
            {
                Time              = Time.time,
                DamageEvents      = m_DamageEventsQuery.ToComponentDataArray<DamageEvent>(Allocator.TempJob),
                EntityType        = entityType,
                DamageHistoryType = damagehistoryType
            };
            var historyHandle = historyJob.Schedule(m_HistoryQuery, inputDeps);

            var addDamageStackJob = new AddDamageStackJob
            {
                ECB               = m_EndSim.CreateCommandBuffer().ToConcurrent(),
                DamageStackBuffer = GetBufferFromEntity<DamageStack>()
            };
            var addDamageStackHandle = addDamageStackJob.Schedule(this, historyHandle);
            m_EndSim.AddJobHandleForProducer(addDamageStackHandle);
            
            var processDamageStackJob = new ProcessDamage();
            var processDamageStackHandle = processDamageStackJob.Schedule(this, addDamageStackHandle);
            
            var tagDeadJob = new TagDeadJob{ EntityCommandBuffer = m_EndSim.CreateCommandBuffer().ToConcurrent()};
            var tagDeadHandle = tagDeadJob.Schedule(this, processDamageStackHandle);
            m_EndSim.AddJobHandleForProducer(tagDeadHandle);
            return tagDeadHandle;
        }
    }
}