using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName = "Unit";

    [Header("HP")]
    public int maxHP = 100;
    public int currentHP;

    [Header("MP")]
    public int maxMP = 50;
    public int currentMP;

    [Header("Stats")]
    public int attack = 20;
    public int defense = 5;

    [Header("State")]
    public bool isGuarding = false;

    public void ResetForBattle()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        isGuarding = false;
    }

    public int TakeDamage(int rawDamage)
    {
        int damage = Mathf.Max(1, rawDamage - defense);

        if (isGuarding)
        {
            damage = Mathf.CeilToInt(damage * 0.5f);
            isGuarding = false;
        }

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        return damage;
    }

    public int Heal(int amount)
    {
        int oldHP = currentHP;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        return currentHP - oldHP;
    }

    public bool SpendMP(int amount)
    {
        if (currentMP < amount)
            return false;

        currentMP -= amount;
        return true;
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}