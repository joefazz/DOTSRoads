using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace Util
{
    public static class WorldGen
    {
        public const int WorldSize = 4;
        public const int MaxAttempts = 10000;

        public static readonly int3[] CardinalDirections =
        {
            new int3(1, 0, 0),
            new int3(-1, 0, 0),
            new int3(0, 1, 0),
            new int3(0, -1, 0),
            new int3(0, 0, 1),
            new int3(0, 0, -1)
        };

        public static readonly int3[] AllDirections =
        {
            new int3(-1, -1, -1),
            new int3(-1, -1, 0),
            new int3(-1, -1, 1),
            new int3(-1, 0, -1),
            new int3(-1, 0, 0),
            new int3(-1, 0, 1),
            new int3(-1, 1, -1),
            new int3(-1, 1, 0),
            new int3(-1, 1, 1),
            new int3(0, -1, -1),
            new int3(0, -1, 0),
            new int3(0, -1, 1),
            new int3(0, 0, -1),
            new int3(0, 0, 1),
            new int3(0, 1, -1),
            new int3(0, 1, 0),
            new int3(0, 1, 1),
            new int3(1, -1, -1),
            new int3(1, -1, 0),
            new int3(1, -1, 1),
            new int3(1, 0, -1),
            new int3(1, 0, 0),
            new int3(1, 0, 1),
            new int3(1, 1, -1),
            new int3(1, 1, 0),
            new int3(1, 1, 1),
        };

        public static NativeArray<bool> GenerateVoxels()
        {
            var random = Random.CreateFromIndex(11111);

            // [ SETUP ]
            // flattening 3-dimensional voxels
            var voxelCount = WorldSize * WorldSize * WorldSize;
            // true indicates presence of an intersection
            var voxels = new NativeArray<bool>(voxelCount, Allocator.Persistent);
            // active voxels will be considered to have new neighboring intersections
            var activeVoxels = new NativeList<int>(Allocator.Persistent);

            // Setup middle voxel
            var middle = GetVoxelIndex(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            voxels[middle] = true;
            activeVoxels.Add(middle);

            // [ DO ALL THE THINGS ]
            var attempts = 0;
            while (attempts < MaxAttempts && activeVoxels.Length > 0)
            {
                attempts++;
                var index = random.NextInt(activeVoxels.Length);
                var activeNode = activeVoxels[index];

                // Select a cardinal neighbor
                var neighborsOfActive = GetCardinalNeighbors(GetVoxelCoords(activeNode));
                var possibleNewNode = neighborsOfActive[random.NextInt(neighborsOfActive.Length)];

                // Check for maximum 2 neighbors in all directions (enforces planar constraint)
                int possibleNewNeighborCount = 0;
                foreach (var n in GetAllNeighbors(possibleNewNode))
                {
                    if (voxels[GetVoxelIndex(n)]) possibleNewNeighborCount++;
                }

                if (possibleNewNeighborCount <= 2)
                {
                    int newNodeIndex = GetVoxelIndex(possibleNewNode);
                    voxels[newNodeIndex] = true;
                    activeVoxels.Add(newNodeIndex);
                }

                // Check if this new node has over-populated last node
                // (and if we need to deactivate it)
                int recountedNeighborsOldNode = 0;
                foreach (var n in GetAllNeighbors(activeNode))
                {
                    if (voxels[GetVoxelIndex(n)]) recountedNeighborsOldNode++;
                }

                if (recountedNeighborsOldNode >= 3)
                {
                    activeVoxels.RemoveAt(index);
                }

            }

            return voxels;
        }

        public static int GetVoxelIndex(int x, int y, int z)
        {
            return x + WorldSize * y + WorldSize * WorldSize * z;
        }

        public static int GetVoxelIndex(int3 pos)
        {
            return GetVoxelIndex(pos.x, pos.y, pos.z);
        }

        public static int3 GetVoxelCoords(int index)
        {
            int3 pos;
            pos.x = index % WorldSize;
            index /= WorldSize;
            pos.y = index % WorldSize;
            index /= WorldSize;
            pos.z = index;
            return pos;
        }

        public static NativeList<int3> GetCardinalNeighbors(int3 pos)
        {
            var neighbors = new NativeList<int3>(Allocator.Persistent);
            foreach (var direction in CardinalDirections)
            {
                var possibleNeighbor = pos + direction;
                if (
                    possibleNeighbor.x > 0 && possibleNeighbor.x < WorldSize
                                           && possibleNeighbor.y > 0 && possibleNeighbor.y < WorldSize
                                           && possibleNeighbor.z > 0 && possibleNeighbor.z < WorldSize)
                {
                    neighbors.Add(possibleNeighbor);
                }
            }

            return neighbors;
        }

        public static NativeList<int3> GetAllNeighbors(int3 pos)
        {
            var neighbors = new NativeList<int3>(Allocator.Persistent);
            foreach (var direction in AllDirections)
            {
                var possibleNeighbor = pos + direction;
                if (
                    possibleNeighbor.x > 0 && possibleNeighbor.x < WorldSize
                                           && possibleNeighbor.y > 0 && possibleNeighbor.y < WorldSize
                                           && possibleNeighbor.z > 0 && possibleNeighbor.z < WorldSize)
                {
                    neighbors.Add(possibleNeighbor);
                }
            }

            return neighbors;
        }
    }
}