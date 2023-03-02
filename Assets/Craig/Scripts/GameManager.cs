using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState : uint
{
    INIT = 0,
    PLAY,
    PAUSE,
    MENU,
    GAME_OVER,
    NUM_OF_STATES
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameState currentState;
    private GameState newState;

    public static GameManager Instance
    {
        get
        {
            if(instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    public GameState CurrentState { get => currentState; }
    public GameState NewState
    {
        get => newState; set
        {
            newState = value;
            DoStateTransition();
        }
    }

    private void Awake()
    {
        currentState = GameState.INIT;
        newState = GameState.INIT;
    }

    void DoStateTransition()
    {
        switch (newState)
        {
            case GameState.INIT:
                break;
            case GameState.PLAY:
                if(currentState == GameState.INIT)
                {
                    if (!SaveLoadManager.Instance.LoadGameFromLocal())
                    {
                        SaveLoadManager.Instance.SaveGameToLocal();
                    }
                    TrigQuestionManager.Instance.NextQuestion();
                    currentState = NewState;
                }
                break;
            case GameState.PAUSE:
                break;
            case GameState.MENU:
                break;
            case GameState.GAME_OVER:
                break;
            case GameState.NUM_OF_STATES:
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.INIT:
                NewState = GameState.PLAY;
                break;
            case GameState.PLAY:
                break;
            case GameState.PAUSE:
                break;
            case GameState.MENU:
                break;
            case GameState.GAME_OVER:
                break;
            case GameState.NUM_OF_STATES:
            default:
                break;
        }
    }
}
