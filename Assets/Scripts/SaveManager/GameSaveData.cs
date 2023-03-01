using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SaveType : int
{
    PLAYER = 0,
    NPC,
    OBJECT,
    NUM_OF_TYPES
}

public enum ObjectType : int
{
    MOVEABLE_BLOCK = 0,
    COLLECTIBLE_POINTS,
    COLLECTIBLE_MULTIPLYER,
    NUM_OF_OBJECTS,
    NOT_AN_OBJECT
}

[Serializable]
public class GameSaveData
{
    public GlobalSaveData globalSaveData = new GlobalSaveData();
    public List<PlayerSaveData> playerSaveData = new List<PlayerSaveData>();
    public List<NPCSaveData> nPCSaveData = new List<NPCSaveData>();
    public List<ObjectSaveData> objectSaveData = new List<ObjectSaveData>();


}
