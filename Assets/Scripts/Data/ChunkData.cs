using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    // The global position of the chunnk. ie (16,16) NOT (1,1). We Want to be able to 
    // acces it as a vector2int, but Vector2Int's are not serialized so we won't be able 
    // to save them. so we will store them as ints.
    int x;
    int y;
    public Vector2Int position
    {
        get { return new Vector2Int(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public ChunkData(Vector2Int pos) { position = pos; }
    public ChunkData(int _x, int _y) { x = _x; y = _y; }

    [HideInInspector] // Displaying lots of data in the inspector slows it down even more so hide this one
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkW, VoxelData.ChunkH, VoxelData.ChunkW];

	public void Populate()
	{
		for (int y = 0; y < VoxelData.ChunkH; y++)
		{
			for (int x = 0; x < VoxelData.ChunkW; x++)
			{
				for (int z = 0; z < VoxelData.ChunkW; z++)
				{
					map[x, y, z] = new VoxelState(World.Instance.GetVoxel(new Vector3(x + position.x, y, z + position.y)));

				}
			}
		}
        World.Instance.worldData.AddToModifiedChunkList(this);
	}
}

