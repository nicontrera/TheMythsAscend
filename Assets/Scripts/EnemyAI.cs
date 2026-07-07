using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Roaming
    }

    private State state;
    private EnemyPathfinding enemyPathfinding;
    public Vector2 roamingPosition;

    public float health, maxHealth = 3f;

    void Awake()
    {
        state = State.Roaming;
        enemyPathfinding = GetComponent<EnemyPathfinding>();
    }

    void Start()
    {
        StartCoroutine(RoamingRoutine());
        health = maxHealth;
    }

    private IEnumerator RoamingRoutine()
    {
        while(state == State.Roaming)
        {
            roamingPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamingPosition);
            yield return new WaitForSeconds(2f);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
