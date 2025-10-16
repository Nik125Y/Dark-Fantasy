using UnityEngine;
using System.Collections;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance; // singleton

    [Header("Respawn Settings")]
    public Transform spawnPoint; // initial spawn point
    public float respawnDelay = 2f;

    private PlayerHealth playerHealth;
    private GameObject playerPrefab;
    private GameObject currentPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // find existing player or load from prefab
        currentPlayer = GameObject.FindWithTag("Player");
        if (currentPlayer != null)
        {
            playerHealth = currentPlayer.GetComponent<PlayerHealth>();
        }
    }

    public void RegisterPlayer(GameObject player)
    {
        currentPlayer = player;
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    public void SetSpawnPoint(Transform newPoint)
    {
        spawnPoint = newPoint;
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(respawnDelay);

        // re-enable player
        currentPlayer.transform.position = spawnPoint.position;
        currentPlayer.SetActive(true);

        // restore movement and physics
        var rb = currentPlayer.GetComponent<Rigidbody2D>();
        var hero = currentPlayer.GetComponent<HeroController>();
        var health = currentPlayer.GetComponent<PlayerHealth>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        hero.enabled = true;
        health.currentHealth = health.maxHealth;
        health.UpdateHearts();

        // small fade-in effect (optional)
        currentPlayer.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
