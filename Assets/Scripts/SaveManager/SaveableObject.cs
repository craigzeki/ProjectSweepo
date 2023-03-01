using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveableObject : MonoBehaviour
{
    [SerializeField] private SaveType saveType;
    [SerializeField] private ObjectType objectType = ObjectType.NOT_AN_OBJECT;
    private bool registeredAsSaveable = false;
    private int myHashIndex = 0;

    public int MyHashIndex { get => myHashIndex; }

    //private PlayerSaveData playerSaveData;
    //private ObjectSaveData objectSaveData;
    //private NPCSaveData nPCSaveData;

    // Start is called before the first frame update
    void Start()
    {
        if(!SaveLoadManager.Instance.RegisterAsSaveable(saveType, this, out myHashIndex))
        {
            Debug.Log("Error: RegisterAsSaveable() failed: SaveType = " + saveType.ToString(), this.gameObject);
        }
        else
        {
            registeredAsSaveable = true;
            Debug.Log(this.gameObject.name + " registered as saveable with SaveType = " + saveType + " : hash index is: " + myHashIndex.ToString(), this.gameObject);
            //switch (saveType)
            //{
            //    case SaveType.PLAYER:
            //        playerSaveData = new PlayerSaveData();
            //        break;
            //    case SaveType.NPC:
            //        nPCSaveData = new NPCSaveData();
            //        break;
            //    case SaveType.OBJECT:
            //        objectSaveData = new ObjectSaveData();
            //        break;
            //    case SaveType.NUM_OF_TYPES:
            //    default:
            //        break;
            //}
        }
    }

    public bool TrySaveData(ref List<PlayerSaveData> playerSaveData)
    {
        if (!registeredAsSaveable) return false;
        if (saveType != SaveType.PLAYER) return false;
        if((myHashIndex == 0) || (myHashIndex > playerSaveData.Count)) return false;

        playerSaveData[myHashIndex].position = transform.position;
        playerSaveData[myHashIndex].scale = transform.localScale;
        playerSaveData[myHashIndex].rotation = transform.rotation.eulerAngles;
        playerSaveData[myHashIndex].nosePosition = gameObject.GetComponent<PlayerMovement>().nose.position;
        playerSaveData[myHashIndex].objectType = objectType;
        playerSaveData[myHashIndex].valid = true;
        return true;
    }

    public bool TrySaveData(ref List<NPCSaveData> nPCSaveData)
    {
        if (!registeredAsSaveable) return false;
        if (saveType != SaveType.NPC) return false;
        if ((myHashIndex == 0) || (myHashIndex > nPCSaveData.Count)) return false;
        //TODO - nPCSaveData = ?

        return true;       

    }

    public bool TrySaveData(ref List<ObjectSaveData> objectSaveData)
    {
        if (!registeredAsSaveable) return false;
        if(saveType != SaveType.OBJECT) return false;
        if ((myHashIndex == 0) || (myHashIndex > objectSaveData.Count)) return false;

        objectSaveData[myHashIndex].position = transform.position;
        objectSaveData[myHashIndex].scale = transform.localScale;
        objectSaveData[myHashIndex].rotation = transform.rotation.eulerAngles;
        objectSaveData[myHashIndex].objectType = objectType;
        objectSaveData[myHashIndex].valid = true;
        return true;
    }

    public bool TryLoadData(List<PlayerSaveData> playerSaveData, int index)
    {
        if(playerSaveData.Count <= index) return false;
        if (!playerSaveData[index].valid) return false;
        if(saveType != SaveType.PLAYER) return false;

        transform.position = playerSaveData[index].position;
        transform.eulerAngles = playerSaveData[index].rotation;
        transform.localScale = playerSaveData[index].scale;
        gameObject.GetComponent<PlayerMovement>().nose.position = playerSaveData[index].nosePosition;
        objectType = playerSaveData[index].objectType;
        return true;
    }

    public bool TryLoadData(List<ObjectSaveData> objectSaveData, int index)
    {
        if(objectSaveData.Count <= index) return false;
        if (!objectSaveData[index].valid) return false;
        if (saveType != SaveType.OBJECT) return false;

        transform.position = objectSaveData[index].position;
        transform.eulerAngles = objectSaveData[index].rotation;
        transform.localScale = objectSaveData[index].scale;
        objectType = objectSaveData[index].objectType;

        return true;
    }

    public void UnRegisterSaveable()
    {
        if(!SaveLoadManager.Instance.UnRegisterAsSaveable(saveType, myHashIndex)) Debug.Log("Error when UnRegistering " + this.gameObject.name + " from SaveLoadManager");
    }

}
