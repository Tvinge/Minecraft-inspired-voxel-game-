using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{//bools with PascalCase
    
    public Settings settings;

    [Header("World Generation Values")]

    public BiomeAttributes[] biomes;

    [Header("Shader")]
    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public Vector3 spawnPosition;
    public Material material;
    public Material transparentMaterial;
   
    public BlockType[] blocktypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord> ();//coordinates in chunks 
    public ChunkCoord playerChunkCoord; 
    ChunkCoord playerLastChunkCoord;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    //simillar to list, can only remove and add first and last object in queue
    public Queue<Chunk> chunksToDraw = new Queue<Chunk> ();

    private bool IsCreatingChunks;

    bool ApplayingModifications = false;

    //Queue in Queue!!!!
    //allows to return whole structures instead of single blocks in structure script
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>> ();

    //private bool _inUI = false;



    public Clouds clouds;

    public GameObject debugScreen;



    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object ();
    public object ChunkListThreadLock = new object();

    private static World _instance;
    public static World Instance { get { return _instance; } }

    public WorldData worldData;

    public string appPath;

    private void Awake()
    {
        // If the instance value is not null and not *this*. we've somehow ended up with more than one World component
        // Since another one has already been assigned, delete this one.

        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        // Else set this to the instance
        else
            _instance = this;

        appPath = Application.persistentDataPath;

    }


    private void Start()
    {
        Debug.Log("Generating new world using seed " + VoxelData.seed);

        worldData = SaveSystem.LoadWorld("Prototype");

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(VoxelData.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);



        LoadWorld();

        SetGlobalLightLevel();
        spawnPosition = new Vector3(VoxelData.WorldCentre, VoxelData.ChunkH - 50f, VoxelData.WorldCentre);
        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        if (settings.EnableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
    }
    public void SetGlobalLightLevel()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }
    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3 (player.position);



        //update the chunks if the player has moved from the chunk ther were previously on
        if(!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToDraw.Count > 0)
            //loooks at the item without removing it
            chunksToDraw.Dequeue().CreateMesh();

        if (!settings.EnableThreading)
        {

            if (!ApplayingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
        /*
        if (Input.GetKeyDown(KeyCode.F1))
            SaveSystem.SaveWorld(worldData);*/
    }
    void LoadWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));

            }
        }
    }
    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }

    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        //lock list to ensure one thing is using the list at a time
        lock (ChunkUpdateThreadLock)
        {
            //make sure update already contain chunk
            if (!chunksToUpdate.Contains(chunk))
            {
                if (insert)
                    chunksToUpdate.Insert(0, chunk);
                else
                    chunksToUpdate.Add(chunk);
            }
        }
    }
    void UpdateChunks()
    {

        lock (ChunkUpdateThreadLock)
        {
            chunksToUpdate[0].UpdateChunk();
            if (!activeChunks.Contains(chunksToUpdate[0].coord))
                activeChunks.Add(chunksToUpdate[0].coord);
            chunksToUpdate.RemoveAt(0);
        }
    }
    void ThreadedUpdate()
    {
        while (true){
            if (!ApplayingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }
    private void OnDisable()
    {
        if (settings.EnableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    void ApplyModifications()
    {
        ApplayingModifications = true;
        
        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                worldData.SetVoxel(v.position, v.id);
            }
        }
        ApplayingModifications = false;
    }
    /* no longer needed, left for science purposes
    IEnumerator CreateChunks() //courutine -  once per frame 
    {
        IsCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;  
        }

        IsCreatingChunks = false;
    }*/
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkW);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkW);
        return new ChunkCoord(x, z);
    }
    public Chunk GetChunkFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkW);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkW);
        return chunks[x, z];
    }
    void CheckViewDistance()
    {
        clouds.UpdateClouds();

        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord; 
        List<ChunkCoord> previuouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();
        //loop though all chunks currently within view distance of the player
        for (int x = coord.x - settings.viewDistance ; x < coord.x + settings.viewDistance ; x++) 
        {
            for (int z = coord.z - settings.viewDistance ; z < coord.z + settings.viewDistance ; z++)
            {   
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                //if the current chunk is in the world 
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {   
                    //check if its active, if not activate it 
                    if (chunks[x, z] == null)
                        chunks[x, z] = new Chunk(thisChunkCoord); 
                    
                    chunks[x, z].IsActive = true;
                    activeChunks.Add(thisChunkCoord);
                }
                // check though previously active chunks to see if this chunk is there. if it is, remove it form the list
                for (int i = 0; i < previuouslyActiveChunks.Count; i++)
                {
                    if (previuouslyActiveChunks[i].Equals(thisChunkCoord))
                        previuouslyActiveChunks.RemoveAt(i);
                }
            }
        }
        //any chunks left in the previousActiveChunks list are no longer in the platyers view distance, so loop through and disable them
        foreach(ChunkCoord c in previuouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
            //activeChunks.Remove(new ChunkCoord(c.x, c.z));//removes chunks not in tutorial
        }
    }
    public bool CheckForVoxel(Vector3 pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);

        if (blocktypes[voxel.id].IsSolid)
            return true;
        else
            return false;
    }
    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }
   /* public bool InUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryWindow.SetActive(true);
                CursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                curstor.Slot.SetActive(false);
            }
        }
    }*/
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        //IMMUTABLE PASS

        //if outside world, return air 
        if (!IsVoxelInWorld(pos))
            return 0;

        //if bottom block of chunk, return bedrock
        if (yPos == 0)
            return 1;

        //BIOME SELECTION PASS

        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

                //keep track of which weight is strongest
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }
            //Get the height of the terrain ( for the current biome) and muliply it by tis weight
            float height = biomes[i].terrainH * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;

            //if the height value is greater = add it to the sum of heights
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }

        //set biome to the one with the strongest weight
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        //get the avarage of the hegihts
        sumOfHeights /= count;

        int terrainH = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

        //BiomeAttributes biome = biomes[index];

        //BASIC TERRAIN PASS
        byte voxelValue = 0;

        if (yPos == terrainH)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrainH && yPos > terrainH - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos > terrainH)
            return 0;
        else
            voxelValue = 2;

        //SECOND PASS
        if (voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        //TREE PASS
        if (yPos == terrainH && biome.PlaceMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFLora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
                }
            }

        }
        return voxelValue;

    }
    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return
                false;
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkH && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }
}



[System.Serializable]
//change it to enum or scriptable object just like with biome?
public class BlockType
{
    public string blockName;
    public bool IsSolid;
    public bool RenderNeighbourFaces;
    public byte opacity;
    public Sprite Icon;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int botFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    //back, front, top, bot, left, right
    public int GetTextureID(int faceIndex)
    {
        //todo bool isTheSameOnAllSides if true u need to specify only 1 value
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return botFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}
public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }
    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }

}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version = "0.0.0.01";

    [Header("Performance")]
    public int loadDistance = 16;
    public bool EnableThreading = true;//DONT CHANGE IN RUNTIME!!!(only when you build the game, unity editor will have no problems
    public int viewDistance = 8;
    public CloudStyle clouds = CloudStyle.Fast;
    public bool EnableChunkLoadAnimation = false;

    [Header("Controls")]
    [Range(0.5f, 10f)]
    public float mouseSensitivity = 2.0f;
    public bool AutoJumpOn = false;


}
