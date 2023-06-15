using Unity.Entities;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LevelController : MonoBehaviour
{
    #region Singleton
    public static LevelController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("Player Values")]
    [SerializeField] private int defaultHealth;
    [SerializeField] private int defaultScore;
    
    public delegate void OnGameStateChangeDelegate();
    public event OnGameStateChangeDelegate OnGameIsLost;
    public event OnGameStateChangeDelegate OnGameIsRestarted;
    
    public delegate void EnemyCollisionDelegate(RefRO<EnemyCharacteristicData> enemyCharacteristicData); 
    public event EnemyCollisionDelegate OnEnemyOutOfBounds;
    public event EnemyCollisionDelegate OnEnemyCollidedWithPlayer;

    public int GetPlayerDefaultHealth() => defaultHealth;
    public int GetPlayerDefaultScore() => defaultScore;

    private EntityManager _entityManager;
    private Entity _gameOverEntity;
    private bool _gameIsLost;

    private void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        _entityManager = world.EntityManager;
        _gameOverEntity = _entityManager.CreateEntity(); 
    }

    private void OnEnable()
    {
        OnGameIsLost += SetupGameOverEventData;
        OnGameIsRestarted += RemoveGameOverEventData;
    }

    private void OnDisable()
    {
        OnGameIsLost -= SetupGameOverEventData;
        OnGameIsRestarted -= RemoveGameOverEventData;
    }

    private void Update()
    {
        if (!_gameIsLost) return;
        
        _entityManager.AddComponentData(_gameOverEntity, new GameOverEventData());
        _gameIsLost = false;
    }
    
    /// <summary>
    /// Invokes the OnGameIsLost event if it has any subscribers.
    /// </summary>
    public void GameIsLost()
    {
        OnGameIsLost?.Invoke();
    }

    /// <summary>
    /// Invokes the event delegate to notify listeners that an enemy has gone out of bounds.
    /// </summary>
    /// <param name="enemyCharacteristicData">The enemy characteristic data associated with the out-of-bounds enemy.</param>
    public void EnemyOutOfBoundsDelegate(RefRO<EnemyCharacteristicData> enemyCharacteristicData)
    {
        OnEnemyOutOfBounds?.Invoke(enemyCharacteristicData);
    }

    /// <summary>
    /// Invokes the event delegate to notify listeners that an enemy has collided with the player.
    /// </summary>
    /// <param name="enemyCharacteristicData">The enemy characteristic data associated with the collided enemy.</param>
    public void EnemyCollidedWithPlayer(RefRO<EnemyCharacteristicData> enemycharacteristicdata)
    {
        OnEnemyCollidedWithPlayer?.Invoke(enemycharacteristicdata);
    }

    /// <summary>
    /// Invokes the event delegate to notify listeners that the game has been restarted.
    /// Called by the button in the GameOverView.
    /// </summary>
    public void GameIsRestarted()
    {
        OnGameIsRestarted?.Invoke();
    }

    private void SetupGameOverEventData()
    {
        _gameIsLost = true;

        //_entityManager.AddComponentData(_gameOverEntity, new GameOverEventData());
    }

    private void RemoveGameOverEventData()
    {
        _entityManager.RemoveComponent<GameOverEventData>(_gameOverEntity);
        //_entityManager.AddComponentData(_gameOverEntity, new GameOverEventData());
    }
}