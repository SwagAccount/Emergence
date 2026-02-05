using UnityEngine;
using System.IO;

public static class Economy
{
    public static int Money { get; private set; }

    static string SavePath =>
        Application.persistentDataPath + "/economy.json";

    public static void Initialize()
    {
        Load();
        Save();
    }

    public static void AddMoney(int amount)
    {
        Money += amount;
        Save();
    }

    public static bool TrySpendMoney(int amount)
    {
        if (Money < amount)
            return false;

        Money -= amount;
        Save();
        return true;
    }

    public static void SetMoney(int amount)
    {
        Money = Mathf.Max(0, amount);
        Save();
    }

    static void Save()
    {
        File.WriteAllText(SavePath, Money.ToString());
    }

    static void Load()
    {
        if (!File.Exists(SavePath))
        {
            Money = 0;
            return;
        }

        int.TryParse(File.ReadAllText(SavePath), out var money);

        Money = money;
    }
}