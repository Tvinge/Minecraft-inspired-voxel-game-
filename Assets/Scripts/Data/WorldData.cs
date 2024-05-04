using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData 
{
    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();

    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new List<ChunkData>();

    public void AddToModifiedChunkList (ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);
    }
    public WorldData(string _worldName, int _seed)
    {
        worldName = _worldName;
        seed = _seed;
    }
    public WorldData ( WorldData wD)
    {
        worldName = wD.worldName;
        seed = wD.seed;
    }
    public ChunkData RequestChunk (Vector2Int coord, bool create)
    {
        ChunkData c;

        lock (World.Instance.ChunkListThreadLock)
        {
            if (chunks.ContainsKey(coord))
                return chunks[coord];
            else if (!create)
                c = null;
            else
            {
                LoadChunk(coord);
                c = chunks[coord];
            }
        }
        return c;
    }
    public void LoadChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            return;

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null)
        {
            chunks.Add(coord, chunk);
            return;
        }


        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkH && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }

    public void SetVoxel (Vector3 pos, byte value)
    {
        //if the voxel is otside of the world we dont need to do anything wit it 
        if (!IsVoxelInWorld(pos))
            return;

        //find out the chunkCoord value of our voxel's chunk
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkW);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkW);

        //than revese that to tget the postion of the chunk
        x *= VoxelData.ChunkW;
        z *= VoxelData.ChunkW;

        //check if the chunk exists. if not create it
        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        //than creaete a vector3Int with the position of our voxel *within* the chunk 
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //then set the voxel in out chunk 
        chunk.map[voxel.x, voxel.y, voxel.z].id = value;
        AddToModifiedChunkList(chunk);
    }
    public VoxelState GetVoxel (Vector3 pos)
    {
        //if the voxel is otside of the world we dont need to do anything wit it 
        if (!IsVoxelInWorld(pos))
            return null;

        //find out the chunkCoord value of our voxel's chunk
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkW);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkW);

        //than revese that to tget the postion of the chunk
        x *= VoxelData.ChunkW;
        z *= VoxelData.ChunkW;

        //check if the chunk exists. if not create it
        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        //than creaete a vector3Int with the position of our voxel *within* the chunk 
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //then set the voxel in out chunk 
        return chunk.map[voxel.x, voxel.y, voxel.z];
    }
}
