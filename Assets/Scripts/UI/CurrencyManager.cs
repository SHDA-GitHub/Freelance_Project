using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public delegate void CurrencyChanged(float newAmount);
    public event CurrencyChanged OnCurrencyChanged;
    public float dollars = 0;

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

    public void AddCoins(float amount)
    {
        if (amount < 0) return;
        dollars += amount;
        OnCurrencyChanged?.Invoke(dollars);
        Debug.Log($"Added {amount} coins. Total: {dollars}");
    }

    public bool SpendCoins(float amount)
    {
        if (amount > dollars) return false;

        dollars -= amount;
        OnCurrencyChanged?.Invoke(dollars);
        Debug.Log($"Spent {amount} coins. Remaining: {dollars}");
        return true;
    }

    public float GetCoinCount()
    {
        return dollars;
    }
}