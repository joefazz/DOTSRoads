using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [BurstCompile]
    partial struct CarBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<CarBootstrap>();

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var cars = CollectionHelper.CreateNativeArray<Entity>(config.CarsToInitialise, Allocator.Temp);

            ecb.Instantiate(config.CarPrefab, cars);

            var tempEntity = ecb.CreateEntity();
            ecb.AddComponent<Lane>(tempEntity);
            var buffer = ecb.AddBuffer<CarDynamicBuffer>(tempEntity).Reinterpret<Entity>();

            buffer.AddRange(cars);

            var nonLaneCars = CollectionHelper.CreateNativeArray<Entity>(100, Allocator.Temp);
            ecb.Instantiate(config.CarPrefab, nonLaneCars);

            state.Enabled = false;
        }
    }
}