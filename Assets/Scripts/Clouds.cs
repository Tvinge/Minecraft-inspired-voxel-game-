using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int cloudH = 100;
    public int cloudDepth = 4;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;

    bool[,] cloudData; // array of bools represetning where colud is 


    int cloudTexW;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject> ();

    private void Start()
    {
        cloudTexW = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkW;
        offset = new Vector3Int(-(cloudTexW / 2), 0, - (cloudTexW / 2));

        transform.position = new Vector3(VoxelData.WorldCentre, cloudH, VoxelData.WorldCentre);
        
        LoadCloudData();
        CreateClouds();
    }

    private void LoadCloudData()
    {
        cloudData = new bool[cloudTexW, cloudTexW];
        Color[] cloudTex = cloudPattern.GetPixels();

        //loop through color array and set bools depending on opacity of coulor
        for (int x = 0; x < cloudTexW; x++)
        {
            for (int y = 0; y < cloudTexW; y++)
            {
                cloudData[x, y] = (cloudTex[y * cloudTexW + x].a > 0);
            }
        }
    }
    private void CreateClouds()
    {
        if (world.settings.clouds == CloudStyle.Off)
            return;
        for (int x = 0; x < cloudTexW; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexW; y += cloudTileSize)
            {
                Mesh cloudMesh;
                if (world.settings.clouds == CloudStyle.Fast)
                    cloudMesh = CreateFastCloudMesh(x, y);
                else
                    cloudMesh = CreateFancyCloudMesh(x, y);

                Vector3 position = new Vector3(x, cloudH, y);
                position += transform.position - new Vector3 (cloudTexW / 2f, 0f, cloudTexW / 2f);
                position.y = cloudH;
                clouds.Add(CloudTilePosFromV3(position), CreateCloudTIle(cloudMesh, position));
            }
        }

    }
    public void UpdateClouds()
    {
        if (world.settings.clouds == CloudStyle.Off)
            return;

        for (int x = 0; x < cloudTexW; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexW; y += cloudTileSize)
            {
                Vector3 position = world.player.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudH, RoundToCloud(position.z));
                Vector2Int cloudPosition = CloudTilePosFromV3(position);

                clouds[cloudPosition].transform.position = position;
            }
        }

    }
    private int RoundToCloud(float value)
    {
        return Mathf.FloorToInt(value / cloudTileSize) * cloudTileSize;
    }
    private Mesh CreateFastCloudMesh (int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {

                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {



                    //add four vertices for cloud face
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));

                    //we know what direction our faces are.... facing, so we just add them directly.
                    for (int i = 0; i < 4; i++)
                        normals.Add(Vector3.down);

                    //add first triangle
                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);
                    //add second triangle
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);
                    //increment vertCount;
                    vertCount += 4;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }
    //biome specific clouds, weather and diffrent types of clouds

    //returns true or false depending on if there is cloud at the given point
    private bool CheckCloudData(Vector3Int point)
    {
        //Becouse clouds are 2D, if y is above or below 0, return false
        if (point.y != 0)
            return false;
        int x = point.x;
        int z = point.z;

        //if the x or z value is outside of the cloudData range, wrap it around
        if (point.x < 0) x = cloudTexW - 1;
        if (point.x > cloudTexW - 1) x = 0;
        if (point.z < 0) z = cloudTexW - 1;
        if (point.z > cloudTexW - 1) z = 0;

        return cloudData[x, z];
    }

    private Mesh CreateFancyCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {

                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {
                    //loop through neighbour points using faceCheck array
                    for (int p = 0; p < 6; p++)
                    {
                        //if the current neighbour has no cloud, draw this face
                        if (!CheckCloudData(new Vector3Int(xVal, 0, zVal) + VoxelData.faceChecks[p]))
                        {
                            //add our four vertices for this face
                            for (int i = 0; i < 4; i++)
                            {
                                Vector3 vert = new Vector3Int(xIncrement, 0, zIncrement);
                                vert += VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                                vert.y *= cloudDepth;
                                vertices.Add(vert);
                            }
                            //we know what direction our faces are.... facing, so we just add them directly.
                            for (int i = 0; i < 4; i++)
                                normals.Add(VoxelData.faceChecks[p]);

                            triangles.Add(vertCount);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 3);

                            vertCount += 4;
                        }
                    }
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }
    private GameObject CreateCloudTIle (Mesh mesh, Vector3 position)
    {
        GameObject newCLoudTile = new GameObject();
        newCLoudTile.transform.position = position;
        newCLoudTile.transform.parent = transform;
        newCLoudTile.name = "Cloud " + position.x + ", " + position.z;
        MeshFilter mF = newCLoudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = newCLoudTile.AddComponent<MeshRenderer>();

        mR.material = cloudMaterial;
        mF.mesh = mesh;

        return newCLoudTile;
    }

    private Vector2Int CloudTilePosFromV3 (Vector3 pos)
    {
        return new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));
    }
    private int CloudTileCoordFromFloat(float value)
    {
        float a = value / (float)cloudTexW;//gets the position using cloudTexture width as units 
        a -= Mathf.FloorToInt(a); //subtract whole numbers to get a 0--1 value represnting position in c loud texture 
        int b = Mathf.FloorToInt((float)cloudTexW * a); //multiply cloud texture width by a to get postition in texture globally

        return b;
    }
}

//smth like list of options?
public enum CloudStyle
{
    Off,
    Fast,
    Fancy
}