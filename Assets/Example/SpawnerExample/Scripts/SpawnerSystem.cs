using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Example
{
    /*  主线程示例
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Queries for all Spawner components. Uses RefRW because this system wants
            // to read from and write to the component. If the system only needed read-only
            // access, it would use RefRO instead.
            foreach (RefRW<Spawner> spawner in SystemAPI.Query<RefRW<Spawner>>())
            {
                ProcessSpawner(ref state, spawner);
            }
        }

        private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawner)
        {
            // If the next spawn time has passed.
            if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
            {
                // Spawns a new entity and positions it at the spawner.
                Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                var pos =UnityEngine.Random.Range(0f, 10f);
                state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition+
                    new Unity.Mathematics.float3(pos, pos, pos)));

                // Resets the next spawn time.
                spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
            }
        }
    }
    */

    // job system 示例
    [BurstCompile]
    public partial struct OptimizedSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
            new ProcessSpawnerJob
            {
                ElapsedTime = SystemAPI.Time.ElapsedTime,
                Ecb = ecb,
                seed = (uint)(Time.time * 1000 + 1)
            }.ScheduleParallel();
        }

        private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            return ecb.AsParallelWriter();
        }
    }

    [BurstCompile]
    public partial struct ProcessSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;
        public uint seed;

        private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
        {
            // If the next spawn time has passed.
            if (spawner.NextSpawnTime < ElapsedTime)
            {
                // Spawns a new entity and positions it at the spawner.
                Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
                var pos = new Unity.Mathematics.Random(seed).NextFloat3(float3.zero,new float3(10f,10f,10f));
                Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition+pos));

                // Resets the next spawn time.
                spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
            }
        }
    }


}

