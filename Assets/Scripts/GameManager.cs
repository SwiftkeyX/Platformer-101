using System;
using UnityEngine;

public enum GameState { Uninitialized, Playing, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Uninitialized;

    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:      Time.timeScale = 1f; break;
            case GameState.Paused:       Time.timeScale = 0f; break;
            case GameState.Uninitialized: Time.timeScale = 1f; break;
        }

        OnGameStateChanged?.Invoke(newState);
    }
}
