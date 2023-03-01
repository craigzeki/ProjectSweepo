using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEditor;
using TMPro;
//using Unity.VisualScripting;

public class SaveLoadManager : MonoBehaviour
{
    private class ObjectHashTable
    {
        public Dictionary<int, SaveableObject> objectHash = new Dictionary<int, SaveableObject>();
        public int nextIndex = 1; //0 used for invalid

        public void DestroySaveables()
        {
            foreach (var go in objectHash.Values)
            {
                if(go != null) Destroy(go.gameObject);
            }
            ClearHashTable();
        }

        public void ClearHashTable()
        {
            objectHash.Clear();
            nextIndex = 1;
        }

        
        public bool RemoveSaveable(int hashIndex)
        {
            //remove the saveable reference - but maintain the hashIndex in case any async processes are running - do not want to change length of dictionary
            objectHash[hashIndex] = null;


            return objectHash[hashIndex] == null ? true : false; 
            //ideally the Dictionary should be cleaned up if no async tasks are running - but this requires a command pattern / async queue
        }


        public bool AddSaveable(SaveableObject saveableObject, out int hashIndex)
        {
            bool result = false;
            hashIndex = 0;

            if (!objectHash.ContainsValue(saveableObject))
            {
                objectHash.Add(nextIndex, saveableObject);
                hashIndex = nextIndex;
                nextIndex++;
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }


    private static SaveLoadManager instance;

    [SerializeField] private GameSaveData gameSaveData = new GameSaveData();
    [SerializeField] private GameSaveData gameLoadData = new GameSaveData();
    [EnumNamedArray(typeof(SaveType))] 
    [SerializeField] private GameObject[] saveTypePrefabs = new GameObject[(int)SaveType.NUM_OF_TYPES];
    [EnumNamedArray(typeof(ObjectType))]
    [SerializeField] private GameObject[] objectTypePrefabs = new GameObject[(int)ObjectType.NUM_OF_OBJECTS];
    [SerializeField] private GameObject savingLoadingIcon;
    [SerializeField] private TextMeshProUGUI statusMessage;
    
    private Dictionary<SaveType, ObjectHashTable> hashTables = new Dictionary<SaveType,ObjectHashTable>();
    private string textToParse;
    private GameSaveData downloadedGameSaveData;

    private string gameSaveDataPath;
    private JsonBinIo jsonBinIo;
    private bool loadGameRequested = false;
    private bool saveGameRequested = false;

    public static SaveLoadManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<SaveLoadManager>();
            return instance;
        }
    }

    private void InitSaveGameData()
    {
        hashTables.Clear();
        //clear the hash tables
        for(int i = 0; i < (int)SaveType.NUM_OF_TYPES; i++)
        {
            hashTables.Add((SaveType)i, new ObjectHashTable());
        }
        //foreach (SaveType saveType in Enum.GetValues(typeof(SaveType)))
        //{
        //    hashTables.Add(saveType, new ObjectHashTable());


        //}
        //clear the save data
        gameSaveData.playerSaveData.Clear();
        gameSaveData.nPCSaveData.Clear();
        gameSaveData.objectSaveData.Clear();

        //create [0] position in each gameSaveData table (dummy slot - not used)
        gameSaveData.playerSaveData.Add(new PlayerSaveData());
        gameSaveData.nPCSaveData.Add(new NPCSaveData());
        gameSaveData.objectSaveData.Add(new ObjectSaveData());
    }

    private void Awake()
    {
        gameSaveDataPath = Application.persistentDataPath + "/saveData.zek";

        InitSaveGameData();
        InitGameloadData();

        jsonBinIo = new JsonBinIo("635297ae2b3499323be62782");
    }


    public bool RegisterAsSaveable(SaveType saveType, SaveableObject saveableObject, out int hashIndex)
    {
        bool result = false;
        hashIndex = 0;

        ObjectHashTable hashTable;
        if(hashTables.TryGetValue(saveType, out hashTable))
        {
            result = hashTable.AddSaveable(saveableObject, out hashIndex);
            if(result)
            {
                //hash position created, now add same position into gameSaveData;
                switch (saveType)
                {
                    case SaveType.PLAYER:
                        gameSaveData.playerSaveData.Add(new PlayerSaveData());
                        break;
                    case SaveType.NPC:
                        gameSaveData.nPCSaveData.Add(new NPCSaveData());
                        break;
                    case SaveType.OBJECT:
                        gameSaveData.objectSaveData.Add(new ObjectSaveData());
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }
            }
        }
        else
        {
            result = false;
        }

        return result;
    }

    public bool UnRegisterAsSaveable(SaveType saveType, int hashIndex)
    {
        bool result = false;

        if (hashIndex == 0) return false; //cannot remove the first item
        
        ObjectHashTable hashTable;
        if (hashTables.TryGetValue(saveType, out hashTable))
        {
            result = hashTable.RemoveSaveable(hashIndex);
            if (result)
            {
                //hash position marked as null, now mark data as invalid;
                switch (saveType)
                {
                    case SaveType.PLAYER:
                        gameSaveData.playerSaveData[hashIndex].valid = false;
                        break;
                    case SaveType.NPC:
                        gameSaveData.nPCSaveData[hashIndex].valid = false;
                        break;
                    case SaveType.OBJECT:
                        gameSaveData.objectSaveData[hashIndex].valid = false;
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }
            }

        }
        else
        {
            result = false;
        }

        return result;
    }

    private void ClearHashTables()
    {
        foreach (ObjectHashTable objectHashTable in hashTables.Values)
        {
            objectHashTable.ClearHashTable();
        }
    }

    private void DestroyAllSaveables()
    {
        foreach(ObjectHashTable objectHashTable in hashTables.Values)
        {
            objectHashTable.DestroySaveables();
        }
    }

    private void InitGameloadData()
    {
        gameLoadData.playerSaveData.Clear();
        gameLoadData.nPCSaveData.Clear();
        gameLoadData.objectSaveData.Clear();
        
    }

    private string SaveGamePrepareJsonString()
    {
        GameManager.Instance.SaveGlobalData(ref gameSaveData.globalSaveData);
        //collect all saveable data from gameobjects
        for (int saveType = 0; saveType < (int)SaveType.NUM_OF_TYPES; saveType++)
        {
            foreach (SaveableObject saveable in hashTables[(SaveType)saveType].objectHash.Values)
            {
                if (saveable == null) continue; //in case saveable has been destroyed

                switch ((SaveType)saveType)
                {
                    case SaveType.PLAYER:
                        saveable.TrySaveData(ref gameSaveData.playerSaveData);
                        break;
                    case SaveType.NPC:
                        saveable.TrySaveData(ref gameSaveData.nPCSaveData);
                        break;
                    case SaveType.OBJECT:
                        saveable.TrySaveData(ref gameSaveData.objectSaveData);
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }

            }
        }

        return JsonUtility.ToJson(gameSaveData);
    }

    public void SaveGameToLocal()
    {
        System.IO.File.WriteAllText(gameSaveDataPath, SaveGamePrepareJsonString());
    }

    public void RequestSaveGame()
    {
        if (saveGameRequested == true)
        {
            Debug.Log("Cannot request save game - already requested");
            return;
        }
        jsonBinIo.SaveJsonToCloud(SaveGamePrepareJsonString());
        if (jsonBinIo.UpdateBin.State == JsonBinIo.JsonBinIoTaskState.FAILED_TO_START)
        {
            Debug.Log("Failed to start Save Game from JsonBinIO: " + jsonBinIo.UpdateBin.ErrorMessage);
        }
        else
        {
            saveGameRequested = true;
        }
    }

    public void RequestLoadGame()
    {
        if(loadGameRequested == true)
        {
            Debug.Log("Cannot request load game - already requested");
            return;
        }

        LoadGamePrepareScene();
        jsonBinIo.GetJsonFromCloud();
        
        if(jsonBinIo.ReadBin.State == JsonBinIo.JsonBinIoTaskState.FAILED_TO_START)
        {
            Debug.Log("Failed to start Load Game from JsonBinIO: " + jsonBinIo.ReadBin.ErrorMessage);
        }
        else
        {
            loadGameRequested = true;
        }
    }

    private void Update()
    {
        if(loadGameRequested)
        {
            
            switch (jsonBinIo.ReadBin.State)
            {
                case JsonBinIo.JsonBinIoTaskState.NOT_STARTED:
                    break;
                case JsonBinIo.JsonBinIoTaskState.STARTED:
                    break;
                case JsonBinIo.JsonBinIoTaskState.FAILED_TO_START:
                    break;
                case JsonBinIo.JsonBinIoTaskState.COMPLETE:
                    //load the game
                    //consider launching this asynchronously - OK while small data
                    Debug.Log(jsonBinIo.ReadBin.Result);
                    LoadGameFromJson(jsonBinIo.ReadBin.Result);
                    loadGameRequested = false;
                    
                    break;
                case JsonBinIo.JsonBinIoTaskState.ERROR:
                    //report error
                    Debug.Log("Error loading from cloud: " + jsonBinIo.ReadBin.ErrorMessage);
                    loadGameRequested = false;
                    break;
                case JsonBinIo.JsonBinIoTaskState.NUM_OF_STATES:
                    //report error
                    Debug.Log("Error loading from cloud: INVALID STATE");
                    loadGameRequested = false;
                    break;
                default:
                    break;
            }
        }

        if(saveGameRequested)
        {
            switch (jsonBinIo.UpdateBin.State)
            {
                case JsonBinIo.JsonBinIoTaskState.NOT_STARTED:
                    break;
                case JsonBinIo.JsonBinIoTaskState.STARTED:
                    break;
                case JsonBinIo.JsonBinIoTaskState.FAILED_TO_START:
                    break;
                case JsonBinIo.JsonBinIoTaskState.COMPLETE:
                    Debug.Log(jsonBinIo.UpdateBin.Result);
                    saveGameRequested = false;
                    break;
                case JsonBinIo.JsonBinIoTaskState.ERROR:
                    //report error
                    Debug.Log("Error saviong to cloud: " + jsonBinIo.UpdateBin.ErrorMessage);
                    saveGameRequested = false;
                    break;
                case JsonBinIo.JsonBinIoTaskState.NUM_OF_STATES:
                    //report error
                    Debug.Log("Error saving to cloud: INVALID STATE");
                    saveGameRequested = false;
                    break;
                default:
                    break;
            }
        }

        //show hide the saving icon
        savingLoadingIcon.SetActive(loadGameRequested || saveGameRequested);
        if(loadGameRequested)
        {
            statusMessage.text = "Loading...";
        }
        else if(saveGameRequested)
        {
            statusMessage.text = "Saving...";
        }
        else
        {
            statusMessage.text = "";
        }
    }

    private void LoadGameFromJson(string jsonString)
    {
        gameLoadData = JsonUtility.FromJson<GameSaveData>(jsonString);

        if(gameLoadData == null)
        {
            Debug.Log("GameLoadData = Null during LoadGameFromJson - jsonString = " + jsonString);
            return;
        }

        //spawn all objects
        for (int saveType = 0; saveType < (int)SaveType.NUM_OF_TYPES; saveType++)
        {

            switch ((SaveType)saveType)
            {
                case SaveType.PLAYER:
                    for (int i = 1; i < gameLoadData.playerSaveData.Count; i++) //0 slot is reserved to identify errors
                    {
                        if (gameLoadData.playerSaveData[i].valid == false) continue; //an object was destroyed in previous game - do not load now - this will clean up the next save file
                        GameObject go;
                        go = Instantiate(saveTypePrefabs[saveType]);
                        go.GetComponent<SaveableObject>().TryLoadData(gameLoadData.playerSaveData, i);
                    }
                    break;
                case SaveType.NPC:
                     
                    break;
                case SaveType.OBJECT:
                    for(int i = 1; i < gameLoadData.objectSaveData.Count; i++) //0 slot is reserved to identify errors
                    {
                        if (gameLoadData.objectSaveData[i].objectType >= ObjectType.NUM_OF_OBJECTS) continue; //not a valid object to load
                        if (gameLoadData.objectSaveData[i].valid == false) continue; //an object was destroyed in previous game - do not load now - this will clean up the next save file
                        GameObject go;
                        go = Instantiate(objectTypePrefabs[(int)gameLoadData.objectSaveData[i].objectType]);
                        go.GetComponent <SaveableObject>().TryLoadData(gameLoadData.objectSaveData, i);
                    }
                    break;
                case SaveType.NUM_OF_TYPES:
                default:
                    break;
            }

        }

        GameManager.Instance.LoadGlobalData(gameLoadData.globalSaveData);
    }
    private void LoadGamePrepareScene()
    {
        DestroyAllSaveables();
        InitSaveGameData();
        InitGameloadData();
    }
    public void LoadGameFromLocal()
    {
        string jsonData;

        LoadGamePrepareScene();

        //load from file
        if (!System.IO.File.Exists(gameSaveDataPath)) return;
        jsonData = System.IO.File.ReadAllText(gameSaveDataPath);
        LoadGameFromJson(jsonData);
    }
}
