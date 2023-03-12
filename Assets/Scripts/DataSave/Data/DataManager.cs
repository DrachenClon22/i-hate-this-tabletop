using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "game";
    [SerializeField] private bool useEncryption = false;

    private GameData gameData;
    private List<IDataManager> dataManagerObjects;
    private FileDataHandler dataHandler;

    public static DataManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("Found one than more Data Manager in the scene");
        instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, $"{fileName}.table", useEncryption);
        this.dataManagerObjects = FindAllDataManagerObjects();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void SaveGame()
    {
        if (this.gameData == null)
        {
            Debug.LogWarning("No data was found. Back to defaults");
            NewGame();
        }

        foreach (IDataManager idm in dataManagerObjects)
        {
            idm.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();

        if (this.gameData == null)
        {
            Debug.LogWarning("No data was found. Back to defaults");
            NewGame();
        }

        foreach(IDataManager idm in dataManagerObjects)
        {
            idm.LoadData(gameData);
        }
    }

    private List<IDataManager> FindAllDataManagerObjects()
    {
        IEnumerable<IDataManager> dataManagerObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataManager>();

        return new List<IDataManager>(dataManagerObjects);
    }
}
