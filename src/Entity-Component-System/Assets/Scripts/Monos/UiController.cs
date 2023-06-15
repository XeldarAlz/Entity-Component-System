using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public class UiController : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private GameObject gameOverView;
    [SerializeField] private GameObject inGameView;
    
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI inGameScoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    private int _health;
    private int _score;

    private void Awake()
    {
        SetGameOverView(false);
        SetDefaultValues();
    }

    private void OnEnable()
    {
        LevelController.Instance.OnGameIsLost += OnGameIsLost;
        LevelController.Instance.OnGameIsRestarted += OnGameIsRestarted;
        LevelController.Instance.OnEnemyOutOfBounds += OnEnemyOutOfBounds;
        LevelController.Instance.OnEnemyCollidedWithPlayer += OnEnemyCollidedWithPlayer;
    }

    private void OnDisable()
    {
        LevelController.Instance.OnGameIsLost -= OnGameIsLost;
        LevelController.Instance.OnGameIsRestarted -= OnGameIsRestarted;
        LevelController.Instance.OnEnemyOutOfBounds -= OnEnemyOutOfBounds;
        LevelController.Instance.OnEnemyCollidedWithPlayer -= OnEnemyCollidedWithPlayer;
    }

    private void OnGameIsLost()
    {
        finalScoreText.text = $"Final Score: {_score}"; 
        
        SetGameOverView(true);
        SetDefaultValues();
    }

    private void OnGameIsRestarted()
    {
        SetGameOverView(false);
    }
    
    private void OnEnemyOutOfBounds(RefRO<EnemyCharacteristicData> enemycharacteristicdata)
    {
        AddScore(enemycharacteristicdata.ValueRO.Reward);
    }

    private void OnEnemyCollidedWithPlayer(RefRO<EnemyCharacteristicData> enemycharacteristicdata)
    {
        DealDamage(enemycharacteristicdata.ValueRO.Damage);
    }

    private void AddScore(int score)
    {
        _score += score;
        inGameScoreText.text = $"Score: {_score}";
    }

    private void DealDamage(int damage)
    {
        if (_health > damage)
        {
            _health -= damage;
            healthText.text = $"Health: {_health}";
        }
        else
        {
            // Trigger game lost event
            LevelController.Instance.GameIsLost();
        }
    }

    private void SetDefaultValues()
    {
        _health = LevelController.Instance.GetPlayerDefaultHealth();
        healthText.text = $"Health: {_health}";

        _score = LevelController.Instance.GetPlayerDefaultScore();
        inGameScoreText.text = $"Score: {_score}";
    }
    
    private void SetGameOverView(bool active)
    {
        inGameView.SetActive(!active);
        gameOverView.SetActive(active);
    }
}