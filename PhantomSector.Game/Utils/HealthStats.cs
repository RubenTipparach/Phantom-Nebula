using System;

namespace PhantomSector.Game.Utils;

/// <summary>
/// Health statistics tracking system
/// Manages current and starting health, damage, healing, and death state
/// Adapted for floats
/// </summary>
public class HealthStats
{

    public float startingHealth = 100;
    public float overideCurrentHealth = -1;
    public float currentHealth = 0;

    public void Init()
    {
        if (overideCurrentHealth != -1)
        {
            currentHealth = overideCurrentHealth;
        }
        else
        {
            currentHealth = startingHealth;
        }
    }

    public float Health
    {
        get { return currentHealth; }
    }

    public float Percent
    {
        get
        {
            return currentHealth / startingHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Clamp(currentHealth - damage, 0, startingHealth);
    }

    public float TakeDamageFromRemaining(float damage)
    {
        float remainingDamage = damage - currentHealth;

        if(damage > currentHealth)
        {
            currentHealth = Clamp(currentHealth - damage, 0, startingHealth);
            return remainingDamage;
        }
        else
        {
            currentHealth = Clamp(currentHealth - damage, 0, startingHealth);
            return 0;
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth = Clamp(currentHealth + healAmount, 0, startingHealth);
    }

    public bool IsFullHealth
    {
        get
        {
            if (currentHealth > startingHealth) currentHealth = startingHealth;
            return currentHealth == startingHealth;
        }
    }

    public float DamagedValue {
        get {
            return startingHealth - currentHealth;
        }
    }

    public void Kill(){
        currentHealth = 0;
    }

    public bool IsDead { get => currentHealth <= 0; }

    /// <summary>
    /// Helper method to clamp a value between min and max
    /// </summary>
    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}


