using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SaveType : int
{
    Q_TRIG = 0,
    Q_GEOMETRY,
    Q_RATIO,
    //TRIG_SETTINGS,
    //GEOMETRY_SETTINGS,
    //RATIO_SETTINGS,
    SCORES,
    SETTINGS,
    NUM_OF_TYPES
}

//todo remove below once confirming not required
//public enum ObjectType : int
//{
//    MOVEABLE_BLOCK = 0,
//    COLLECTIBLE_POINTS,
//    COLLECTIBLE_MULTIPLYER,
//    NUM_OF_OBJECTS,
//    NOT_AN_OBJECT
//}

[Serializable]
public class GameSaveData
{
    public TrigSettingsData TrigSettingsData = new TrigSettingsData();
    public List<TrigQuestionData> TrigQuestionData = new List<TrigQuestionData>();
    //todo remove below once finished using as exmaple
    //public GlobalSaveData globalSaveData = new GlobalSaveData();
    //public List<PlayerSaveData> playerSaveData = new List<PlayerSaveData>();
    //public List<NPCSaveData> nPCSaveData = new List<NPCSaveData>();
    //public List<ObjectSaveData> objectSaveData = new List<ObjectSaveData>();


}
