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
    private enum StatusTextState
    {
        LOADING = 0,
        SAVING,
        SUCCESSFUL,
        UNSUCCESSFUL,
        NOTHING,
        UNKNOWN,
        NUM_OF_STATES
    }
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
    //[EnumNamedArray(typeof(ObjectType))]
    //[SerializeField] private GameObject[] objectTypePrefabs = new GameObject[(int)ObjectType.NUM_OF_OBJECTS];
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private TextMeshProUGUI _loadSuccessfulText;
    [SerializeField] private TextMeshProUGUI _loadFailedText;
    [SerializeField] private float _statusTextBlinkPeriod = 0.6f;

    private Dictionary<SaveType, ObjectHashTable> hashTables = new Dictionary<SaveType,ObjectHashTable>();
    private string textToParse;
    private GameSaveData downloadedGameSaveData;

    [SerializeField] private string gameSaveDataPath;
    private JsonBinIo jsonBinIo;
    private bool loadGameRequested = false;
    private bool saveGameRequested = false;
    private StatusTextState _statusTextState = StatusTextState.UNKNOWN;
    private Coroutine _statusTextCoroutine;
    private bool _onLoadSaveToLocal = false;


    public event EventHandler SaveablesDestroyed;
    public event EventHandler<bool> CloudLoadComplete;
    public event EventHandler<bool> LocalLoadComplete;
    public event EventHandler<bool> CloudSaveComplete;
    public event EventHandler<bool> LocalSaveComplete;

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
        gameSaveData.TrigQuestionData.Clear();


        //create [0] position in each gameSaveData table (dummy slot - not used)
        gameSaveData.TrigQuestionData.Add(new TrigQuestionData());
        
    }

    private void Awake()
    {
        gameSaveDataPath = Application.persistentDataPath + "/saveData.zek";

        InitSaveGameData();
        InitGameloadData();

        UpdateStatusText(StatusTextState.NOTHING);

        jsonBinIo = new JsonBinIo("63ff7210c0e7653a0580bd3d");
    }

    public GameObject CreateSaveable(SaveType saveType)
    {
        if (saveType >= SaveType.NUM_OF_TYPES) return null;
        if ((int)saveType >= saveTypePrefabs.Length) return null;
        return Instantiate(saveTypePrefabs[(int)saveType]);
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
                    case SaveType.Q_TRIG:
                        gameSaveData.TrigQuestionData.Add(new TrigQuestionData());
                        break;
                    case SaveType.Q_GEOMETRY:
                        break;
                    case SaveType.Q_RATIO:
                        break;
                    case SaveType.SCORES:
                        break;
                    case SaveType.SETTINGS:
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }


                //TODO remove once all are in above
                //switch (saveType)
                //{
                //    case SaveType.Q_T:
                //        gameSaveData.playerSaveData.Add(new PlayerSaveData());
                //        break;
                //    case SaveType.NPC:
                //        gameSaveData.nPCSaveData.Add(new NPCSaveData());
                //        break;
                //    case SaveType.OBJECT:
                //        gameSaveData.objectSaveData.Add(new ObjectSaveData());
                //        break;
                //    case SaveType.NUM_OF_TYPES:
                //    default:
                //        break;
                //}
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
                    case SaveType.Q_TRIG:
                        gameSaveData.TrigQuestionData[hashIndex].valid = false;
                        break;
                    case SaveType.Q_GEOMETRY:
                        break;
                    case SaveType.Q_RATIO:
                        break;
                    case SaveType.SCORES:
                        break;
                    case SaveType.SETTINGS:
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }

                //TODO remove once all are in above
                //switch (saveType)
                //{
                //    case SaveType.PLAYER:
                //        gameSaveData.playerSaveData[hashIndex].valid = false;
                //        break;
                //    case SaveType.NPC:
                //        gameSaveData.nPCSaveData[hashIndex].valid = false;
                //        break;
                //    case SaveType.OBJECT:
                //        gameSaveData.objectSaveData[hashIndex].valid = false;
                //        break;
                //    case SaveType.NUM_OF_TYPES:
                //    default:
                //        break;
                //}
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
        OnSaveablesDestroyed(EventArgs.Empty);
    }

    private void InitGameloadData()
    {
        gameLoadData.TrigQuestionData.Clear();

        
        
    }

    private string SaveGamePrepareJsonString()
    {
        //TODO GameManager.Instance.SaveGlobalData(ref gameSaveData.globalSaveData);
        TrigQuestionManager.Instance.SaveTrigSettings(ref gameSaveData.TrigSettingsData);
        //collect all saveable data from gameobjects
        for (SaveType saveType = 0; saveType < SaveType.NUM_OF_TYPES; saveType++)
        {
            foreach (SaveableObject saveable in hashTables[saveType].objectHash.Values)
            {
                if (saveable == null) continue; //in case saveable has been destroyed

                switch (saveType)
                {
                    case SaveType.Q_TRIG:
                        saveable.TrySaveData(ref gameSaveData.TrigQuestionData);
                        break;
                    case SaveType.Q_GEOMETRY:
                        break;
                    case SaveType.Q_RATIO:
                        break;
                    case SaveType.SCORES:
                        break;
                    case SaveType.SETTINGS:
                        break;
                    case SaveType.NUM_OF_TYPES:
                    default:
                        break;
                }

                //todo remove once all are complete above
                //switch ((SaveType)saveType)
                //{
                //    case SaveType.PLAYER:
                //        saveable.TrySaveData(ref gameSaveData.playerSaveData);
                //        break;
                //    case SaveType.NPC:
                //        saveable.TrySaveData(ref gameSaveData.nPCSaveData);
                //        break;
                //    case SaveType.OBJECT:
                //        saveable.TrySaveData(ref gameSaveData.objectSaveData);
                //        break;
                //    case SaveType.NUM_OF_TYPES:
                //    default:
                //        break;
                //}

            }
        }

        return JsonUtility.ToJson(gameSaveData);
    }

    public void SaveGameToLocal()
    {
        if (gameSaveDataPath == "")
        {
            OnLocalSaveCompleted(false);
            return;
        }
        System.IO.File.WriteAllText(gameSaveDataPath, SaveGamePrepareJsonString());
        OnLocalSaveCompleted(true);
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

    public void RequestLoadGame(bool saveAsLocal = false)
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
            _onLoadSaveToLocal = saveAsLocal;
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
                    UpdateStatusText(StatusTextState.LOADING);
                    break;
                case JsonBinIo.JsonBinIoTaskState.FAILED_TO_START:
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudLoadComplete(false);
                    break;
                case JsonBinIo.JsonBinIoTaskState.COMPLETE:
                    //load the game
                    //consider launching this asynchronously - OK while small data
                    Debug.Log(jsonBinIo.ReadBin.Result);
                    LoadGameFromJson(jsonBinIo.ReadBin.Result);
                    UpdateStatusText(StatusTextState.SUCCESSFUL);
                    if(_onLoadSaveToLocal) SaveGameToLocal();
                    OnCloudLoadComplete(true);
                    loadGameRequested = false;
                    break;
                case JsonBinIo.JsonBinIoTaskState.ERROR:
                    //report error
                    Debug.Log("Error loading from cloud: " + jsonBinIo.ReadBin.ErrorMessage);
                    loadGameRequested = false;
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudLoadComplete(false);
                    break;
                case JsonBinIo.JsonBinIoTaskState.NUM_OF_STATES:
                    //report error
                    Debug.Log("Error loading from cloud: INVALID STATE");
                    loadGameRequested = false;
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudLoadComplete(false);
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
                    UpdateStatusText(StatusTextState.SAVING);
                    break;
                case JsonBinIo.JsonBinIoTaskState.FAILED_TO_START:
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudSaveCompleted(false);
                    break;
                case JsonBinIo.JsonBinIoTaskState.COMPLETE:
                    Debug.Log(jsonBinIo.UpdateBin.Result);
                    saveGameRequested = false;
                    UpdateStatusText(StatusTextState.SUCCESSFUL);
                    OnCloudSaveCompleted(true);
                    break;
                case JsonBinIo.JsonBinIoTaskState.ERROR:
                    //report error
                    Debug.Log("Error saviong to cloud: " + jsonBinIo.UpdateBin.ErrorMessage);
                    saveGameRequested = false;
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudSaveCompleted(false);
                    break;
                case JsonBinIo.JsonBinIoTaskState.NUM_OF_STATES:
                    //report error
                    Debug.Log("Error saving to cloud: INVALID STATE");
                    saveGameRequested = false;
                    UpdateStatusText(StatusTextState.UNSUCCESSFUL);
                    OnCloudSaveCompleted(false);
                    break;
                default:
                    break;
            }
        }

        //show hide the saving icon
        //if (savingLoadingIcon == null) return;
        //savingLoadingIcon.SetActive(loadGameRequested || saveGameRequested);

        //if (statusMessage == null) return;
        //if (loadGameRequested)
        //{
        //    statusMessage.text = "Loading...";
        //}
        //else if(saveGameRequested)
        //{
        //    statusMessage.text = "Saving...";
        //}
        //else
        //{
        //    statusMessage.text = "";
        //}

        
        
    }

    private void UpdateStatusText(StatusTextState state)
    {
        if (state == _statusTextState) return;
        if (_statusTextCoroutine != null) StopCoroutine(_statusTextCoroutine);

        switch (state)
        {
            case StatusTextState.LOADING:
                _loadingText.enabled = true;
                _loadFailedText.enabled = false;
                _loadSuccessfulText.enabled = false;
                
                _statusTextCoroutine = StartCoroutine(BlinkText(_loadingText, _statusTextBlinkPeriod));
                _statusTextState = state;
                break;
            case StatusTextState.SAVING:
                _loadingText.enabled = false;
                _loadFailedText.enabled = false;
                _loadSuccessfulText.enabled = false;

                _statusTextCoroutine = StartCoroutine(BlinkText(_loadingText, _statusTextBlinkPeriod));
                _statusTextState = state;
                break;
            case StatusTextState.SUCCESSFUL:
                _loadingText.enabled = false;
                _loadFailedText.enabled = false;
                _loadSuccessfulText.enabled = true;

                _statusTextCoroutine = StartCoroutine(BlinkText(_loadSuccessfulText, _statusTextBlinkPeriod));
                _statusTextState = state;
                break;
            case StatusTextState.UNSUCCESSFUL:
                _loadingText.enabled = false;
                _loadFailedText.enabled = true;
                _loadSuccessfulText.enabled = false;

                _statusTextCoroutine = StartCoroutine(BlinkText(_loadFailedText, _statusTextBlinkPeriod));
                _statusTextState = state;
                break;
            case StatusTextState.NOTHING:
                _loadingText.enabled = false;
                _loadFailedText.enabled = false;
                _loadSuccessfulText.enabled = false;
                _statusTextState = state;
                break;
            case StatusTextState.UNKNOWN:
            case StatusTextState.NUM_OF_STATES:
                break;

        }
        
    }

    IEnumerator BlinkText(TextMeshProUGUI tmpText, float period)
    {
        while (true)
        {
            yield return new WaitForSeconds(period);
            tmpText.enabled = !tmpText.enabled;
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
        for (SaveType saveType = 0; saveType < SaveType.NUM_OF_TYPES; saveType++)
        {
            switch (saveType)
            {
                case SaveType.Q_TRIG:
                    for (int i = 1; i < gameLoadData.TrigQuestionData.Count; i++)
                    {
                        if (gameLoadData.TrigQuestionData[i].valid == false) continue;
                        GameObject go;
                        go = Instantiate(saveTypePrefabs[(int)saveType]);
                        go.GetComponent<SaveableObject>().TryLoadData(gameLoadData.TrigQuestionData, i);
                    }
                    break;
                case SaveType.Q_GEOMETRY:
                    break;
                case SaveType.Q_RATIO:
                    break;
                case SaveType.SCORES:
                    break;
                case SaveType.SETTINGS:
                    break;
                case SaveType.NUM_OF_TYPES:
                default:
                    break;
            }

            //todo remove below once all above are complete
            //switch ((SaveType)saveType)
            //{
            //    case SaveType.PLAYER:
            //        for (int i = 1; i < gameLoadData.playerSaveData.Count; i++) //0 slot is reserved to identify errors
            //        {
            //            if (gameLoadData.playerSaveData[i].valid == false) continue; //an object was destroyed in previous game - do not load now - this will clean up the next save file
            //            GameObject go;
            //            go = Instantiate(saveTypePrefabs[saveType]);
            //            go.GetComponent<SaveableObject>().TryLoadData(gameLoadData.playerSaveData, i);
            //        }
            //        break;
            //    case SaveType.NPC:
                     
            //        break;
            //    case SaveType.OBJECT:
            //        for(int i = 1; i < gameLoadData.objectSaveData.Count; i++) //0 slot is reserved to identify errors
            //        {
            //            if (gameLoadData.objectSaveData[i].objectType >= ObjectType.NUM_OF_OBJECTS) continue; //not a valid object to load
            //            if (gameLoadData.objectSaveData[i].valid == false) continue; //an object was destroyed in previous game - do not load now - this will clean up the next save file
            //            GameObject go;
            //            go = Instantiate(objectTypePrefabs[(int)gameLoadData.objectSaveData[i].objectType]);
            //            go.GetComponent <SaveableObject>().TryLoadData(gameLoadData.objectSaveData, i);
            //        }
            //        break;
            //    case SaveType.NUM_OF_TYPES:
            //    default:
            //        break;
            //}

        }
        TrigQuestionManager.Instance.LoadTrigSettings(gameLoadData.TrigSettingsData);
        //TODO GameManager.Instance.LoadGlobalData(gameLoadData.globalSaveData);
    }
    private void LoadGamePrepareScene()
    {
        DestroyAllSaveables();
        InitSaveGameData();
        InitGameloadData();
    }
    public bool LoadGameFromLocal()
    {
        string jsonData;
        //load from file
        if (!System.IO.File.Exists(gameSaveDataPath))
        {
            OnLocalLoadCompleted(false);
            return false;
        }

        LoadGamePrepareScene();
        jsonData = System.IO.File.ReadAllText(gameSaveDataPath);
        LoadGameFromJson(jsonData);
        OnLocalLoadCompleted(true);
        return true;
    }

    protected virtual void OnSaveablesDestroyed(EventArgs e)
    {
        SaveablesDestroyed?.Invoke(this, e);
    }

    protected virtual void OnCloudLoadComplete(bool IsSuccessful)
    {
        CloudLoadComplete?.Invoke(this, IsSuccessful);
    }

    protected virtual void OnCloudSaveCompleted(bool IsSuccessful)
    {
        CloudSaveComplete?.Invoke(this, IsSuccessful);
    }

    protected virtual void OnLocalLoadCompleted(bool IsSuccessful)
    {
        LocalLoadComplete?.Invoke(this, IsSuccessful);
    }
    
    protected virtual void OnLocalSaveCompleted(bool IsSuccessful)
    {
        LocalLoadComplete?.Invoke(this, IsSuccessful);
    }
}
