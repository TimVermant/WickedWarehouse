using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Author = Tristan Wauthier
/// <summary>
/// Class that manages the structure of the game.
/// Examples are: the game state, the amount of players etc.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Multiplayer variables")]
    [SerializeField] private int m_AmountOfPlayers = 2;

    [Header("Player spawn variables")]
    [SerializeField] private Transform m_PlayerSpawnAreaTransform;
    [SerializeField] private float m_PlayerSpawnAreaRadius = 4;
    [SerializeField] private GameObject m_PlayerPrefab;
    [Space]
    [Header("Player materials")]
    [SerializeField] private Material m_MaterialPlayer1;
    [SerializeField] private Material m_MaterialPlayer2;
    private List<Material> m_PlayerMaterials = new List<Material>();


    [Header("Level functionality")]
    [SerializeField] private GameObject m_PauseCanvas = null;

    [Space]
    // Game Audio
    [Header("Game background audio")]
    [SerializeField] private AudioSource m_AudioMusic;
    [SerializeField] private AudioSource m_AudioAmbient;
    [Space]
    [Header("Game audio effects")]
    [SerializeField] private AudioSource m_AudioPauseMenu;
    [SerializeField] private AudioSource m_AudioCloseMenu;
    [Space]
    [Header("Player spawn particle systems")]
    [SerializeField] private GameObject m_Player1SpawnPS;
    [SerializeField] private GameObject m_Player2SpawnPS;
    private List<GameObject> m_PlayerspawnPSList = new List<GameObject>();

    private bool m_IsInitialized = false;
    

    private GameState m_State;
    public GameState CurrentState
    {
        get { return m_State; }
    }
    private void Start()
    {
        if (m_PauseCanvas == null || m_PlayerPrefab == null)
            throw new MissingReferenceException("GameManager Start(): Not all components initialized!");

        m_PauseCanvas.SetActive(false);
        ChangeGameState(GameState.Game);

        m_PlayerMaterials.Add(m_MaterialPlayer1);
        m_PlayerMaterials.Add(m_MaterialPlayer2);

        m_PlayerspawnPSList.Add(m_Player1SpawnPS);
        m_PlayerspawnPSList.Add(m_Player2SpawnPS);
    }

    public void Initialize()
    {
        if (m_IsInitialized) return;
        m_IsInitialized = true;

        SpawnPlayers();
        ScoreManager.Instance.CurrentScore = 0.0f;




        // Background music
        if (m_AudioMusic)
            AudioManager.Instance.AddMusic(m_AudioMusic);
        if (m_AudioAmbient)
            AudioManager.Instance.AddMusic(m_AudioAmbient);


        AudioManager.Instance.StartPlayingBackground();
       
    }

    void SpawnPlayers()
    {
        for (int i = 0; i < m_AmountOfPlayers; i++)
        {
            Vector3 spawnPos = CalculateSpawnPos(i);
            Quaternion spawnRotation = CalculateSpawnRotation();
            GameObject player = Instantiate(m_PlayerPrefab, spawnPos, spawnRotation);
            player.GetComponent<PlayerController>().PlayerID = i;

            player.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = m_PlayerMaterials[i];
            Instantiate(m_PlayerspawnPSList[i], spawnPos, spawnRotation);
        }
    }

    public void ChangeGameState(GameState newState)
    {
        if (m_State == GameState.PauseMenu)
        {
            if (m_AudioCloseMenu)
            {
                m_AudioCloseMenu.Play();
                Invoke("ContinueAudio", m_AudioCloseMenu.clip.length);

            }
        }
        Time.timeScale = 1;
        m_PauseCanvas.gameObject.SetActive(false);

        m_State = newState;
        switch (m_State)
        {
            case GameState.MainMenu:
                break;
            case GameState.Game:
                break;
            case GameState.PauseMenu:
                PauseGame();
                break;
            case GameState.GameOver:
                EndGame();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

    }

    private void ContinueAudio()
    {
        AudioManager.Instance.AudioOnGameContinue();
    }

    private void EndGame()
    {
        
        ScoreManager.Instance.CompleteGame();


        LevelLoader.Instance.gameObject.GetComponentInChildren<Image>().color = Color.black;
        LevelLoader.Instance.DoLevelTransition = true;
        


    }

    public void ResumeGame()
    {
        ChangeGameState(GameState.Game);


        AudioManager.Instance.AudioOnGameContinue();
    }

    private void PauseGame()
    {
        print("Pausing menu");

        m_PauseCanvas.gameObject.SetActive(true);
        m_AudioPauseMenu.Play();
        Time.timeScale = 0;
        AudioManager.Instance.AudioOnGamePause();

    }

    public void QuitGame()
    {
        m_PauseCanvas.gameObject.SetActive(false);
        LevelLoader.Instance.m_Level = 0;
        LevelLoader.Instance.DoLevelTransition = true;
    }

    //Spawn utils
    private Vector3 CalculateSpawnPos(int playerID)
    {
        if (m_AmountOfPlayers == 1)
            return m_PlayerSpawnAreaTransform.position;

        float angle = (playerID) * Mathf.PI * 2 / m_AmountOfPlayers;
        float x = Mathf.Cos(angle) * m_PlayerSpawnAreaRadius;
        float z = Mathf.Sin(angle) * m_PlayerSpawnAreaRadius;
        return m_PlayerSpawnAreaTransform.position + new Vector3(x, 0, z);
    }

    private Quaternion CalculateSpawnRotation()
    {
        return Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));
    }
}

public enum GameState
{
    MainMenu,
    Game,
    PauseMenu,
    GameOver
}

