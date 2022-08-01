using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    // [UpdateAfter(typeof(CarEvaluateSplinePositionSystem))]
    [BurstCompile]
    public partial struct EvaluateCarsNextIntersectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            NativeList<CarAspect> carAspects = new NativeList<CarAspect>(32, Allocator.Temp);

            foreach (var carAspect in SystemAPI.Query<CarAspect>().WithAll<WaitingAtIntersection>())
            {
                carAspects.Add(carAspect);
            }
            
            // This issue is that in here cars can be retargetted twice 
            for (int i = 0; i < carAspects.Length; i++)
            {
                foreach (var roadSegment in SystemAPI.Query<RoadSegmentAspect>().WithNone<IntersectionSegment>())
                {
                    var carAspect = carAspects[i];
                    if (roadSegment.StartIntersection == carAspect.NextIntersection && roadSegment.Entity != carAspect.RoadSegmentEntity)
                    {
                        carAspect.NextIntersection = roadSegment.EndIntersection;
                        carAspect.T = 0;
                        carAspect.RoadSegmentEntity = roadSegment.Entity;
                        ecb.SetComponentEnabled<WaitingAtIntersection>(carAspect.Entity, false);
                        i = (i + 1) % carAspects.Length;
                    }
                    else if (roadSegment.EndIntersection == carAspect.NextIntersection && roadSegment.Entity != carAspect.RoadSegmentEntity)
                    {
                        carAspect.NextIntersection = roadSegment.StartIntersection;
                        carAspect.T = 0;
                        carAspect.RoadSegmentEntity = roadSegment.Entity;
                        ecb.SetComponentEnabled<WaitingAtIntersection>(carAspect.Entity, false);
                        i = (i + 1) % carAspects.Length;
                    }
                }
            }
        }
    }
}
