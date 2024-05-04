using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;
    Toolbar toolbar;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    int targetFrameRate = 144;


    void Start()
    {
        
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInChunks= VoxelData.WorldSizeInChunks / 2;
        halfWorldSizeInVoxels= VoxelData.WorldSizeInVoxels / 2;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }

    void Update()
    {
        string debugText = "Dom Wariatow";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + Mathf.FloorToInt(world.player.transform.position.y) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        debugText += "SlotIndex: " + Toolbar.slotIndex;
        debugText += "\n";
        //not working?
        //debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks);
        text.text = debugText;

        if (timer > 1f)
        {//(int) zwraca tylko 1 liczbe bez przecinka 
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }
}
/*I got it working. The issues seems to be that the normal text component is depreciated so you have to use the TMPro instead. Instead of importing "using UnityEngine.UI;" i had to use "using TMPro;" 

then instead of creating the variables "Text text" I created "TextMeshProUGUI textMeshPro;"

Then in the start() function I did "textMeshPro = GetComponent<TextMeshProUGUI>();" instead of the text stuff that was in there

Finally in the update function do "textMeshPro.text = debugText;" instead of "text.text = debugText;"*/