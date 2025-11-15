using System.Collections;
using System.Diagnostics;
using UnityEngine;

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

        // Reset position BEFORE activation
        currentPlayer.transform.position = spawnPoint.position;
        var bc = currentPlayer.GetComponent<BoxCollider2D>();
        if (bc != null) bc.enabled = true;

        // Reset physics BEFORE enabling player
        var rb = currentPlayer.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        

        // Restore gameplay components
        var hero = currentPlayer.GetComponent<HeroController>();
        var health = currentPlayer.GetComponent<PlayerHealth>();

        hero.enabled = true;
        
        health.currentHealth = health.maxHealth;
        health.UpdateHearts();
        health.enabled = true;

        // Optional: reset animator state
        var anim = currentPlayer.GetComponent<Animator>();
        anim.Rebind();
        anim.Update(0f);

        // Now enable player
        currentPlayer.SetActive(true);

        // Ensure player is fully visible
        currentPlayer.GetComponent<SpriteRenderer>().color = Color.white;

    }

}
