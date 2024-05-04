using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;


public class TitleMenu : MonoBehaviour
{

    public GameObject mainMenuObject;
    public GameObject settingsObject;

    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;

    [Header("Settings Menu Ui Elements")]
    public Slider viewDistSlider;
    public TextMeshProUGUI viewDistText;
    public Slider mouseSlider;
    public TextMeshProUGUI mouseTxtSlider;
    public Toggle threadingToggle;
    public Toggle chunkAnimToggle;
    public Toggle autoJumpToggle;
    public TMP_Dropdown clouds;


    Settings settings;

    private void Awake()
    {
        if (!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            Debug.Log("No settings file found, creating new one.");
            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);
        }
        else
        {
            Debug.Log("Settings file found, loading settings.");
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode())/VoxelData.WorldSizeInChunks;
        SceneManager.LoadScene("main", LoadSceneMode.Single);
    }
    public void EnterSettings()
    {
        viewDistSlider.value = settings.viewDistance;
        UpdateViewDistSlider();
        viewDistText.text = "View Distance: " + viewDistSlider.value;
        UpdateMouseSlider();
        mouseSlider.value = settings.mouseSensitivity;
        mouseTxtSlider.text = "Mouse Sensitivity: " + mouseSlider.value;
        threadingToggle.isOn = settings.EnableThreading;
        chunkAnimToggle.isOn = settings.EnableChunkLoadAnimation;
        autoJumpToggle.isOn = settings.AutoJumpOn;
        clouds.value = (int)settings.clouds;

        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }
    public void LeaveSettings()
    {
        settings.viewDistance = (int)viewDistSlider.value;
        settings.mouseSensitivity = mouseSlider.value;
        settings.EnableThreading = threadingToggle.isOn;
        settings.EnableChunkLoadAnimation = chunkAnimToggle.isOn;
        settings.AutoJumpOn = autoJumpToggle.isOn;
        settings.clouds = (CloudStyle)clouds.value;
        
        string jsonExport = JsonUtility.ToJson (settings);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);


        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void UpdateViewDistSlider()
    {
        viewDistText.text = "View Distance: " + viewDistSlider.value;
    }
    public void UpdateMouseSlider()
    {
        mouseTxtSlider.text = "Mouse Sensitivity: " + mouseSlider.value.ToString("F1");
    }
}
