using UnityEngine;

public class Player
{
    public string PlayerName { get; private set; }
    public int Score { get; private set; }
    public int Level { get; private set; }
    public int Rank { get; private set; }

    public Player(string playerName)
    {
        PlayerName = playerName;
        Score = 0;
        Level = 1;
        Rank = 0;
    }

    public void UpdateScore(int points)
    {
        Score += points;
    }

    public void UpdateLevel(int newLevel)
    {
        Level = newLevel;
    }

    public void UpdateRank(int newRank)
    {
        Rank = newRank;
    }
}
