using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

//static - we can acces it from anywhere
public static class SaveSystem
{
    public static void SaveWorld (WorldData world)
    {
        //set our save location and make sure wew have a saves folder ready to go
        string savePath = World.Instance.appPath + "/saves/" + world.worldName + "/";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        Debug.Log("Saving " + world.worldName);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);

        formatter.Serialize(stream, world);
        stream.Close();

        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();
    }
    public static void SaveChunks (WorldData world)
    {
        //copy modified chunks into a new list and clear the old one to prevent 
        //chunks being added to list while it is saving
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();


        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;

        }
        Debug.Log(count + " chunks saved.");
    }
    public static WorldData LoadWorld (string worldName, int seed = 0)
    {
        //path to the word saves
        string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";

        //check if save exists
        if (File.Exists(loadPath + "world.world"))
        {
            Debug.Log(worldName + " found. Loading from save");

            //if it does, load that file, deserialize it, and put it in a WorldData class for return
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);

            //and than return world
            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();

            return new WorldData(world);
        }
        //create and save world if it does not exist
        else
        {
            Debug.Log(worldName + " not found. Creating new world.");

            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);

            return world;
        }
    }
    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = chunk.position.x + "-" + chunk.position.y;

        //set our save location and make sure wew have a saves folder ready to go
        string savePath = World.Instance.appPath + "/saves/" + worldName + "/chunks/";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + chunkName + ".chunk", FileMode.Create);

        formatter.Serialize(stream, chunk);
        stream.Close();
    }
    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = position.x + "-" + position.y;

        string loadPath = World.Instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName + ".chunk";

        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();
            return chunkData;
        }
        return null;
    }
}
