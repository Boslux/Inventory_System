using System;

public class EnemyHealth
{
    public event Action OnDied;
    public event Action OnHit;

    private int health;

    public EnemyHealth(int initialHealth = 10)
    {
        health = Math.Max(0, initialHealth);
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) return;

        health -= Math.Max(0, damage);
        OnHit?.Invoke();

        if (health <= 0)
        {
            health = 0;
            OnDied?.Invoke();
        }
    }
}
