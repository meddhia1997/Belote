using UnityEngine;
using TMPro;

public class ScoreboardView_Text : MonoBehaviour, IScoreboardView
{
    [Header("Text refs")]
    public TMP_Text roundUs;
    public TMP_Text roundThem;
    public TMP_Text matchUs;
    public TMP_Text matchThem;

    public void SetRound(int us, int them)
    {
        if (roundUs) roundUs.text = us.ToString();
        if (roundThem) roundThem.text = them.ToString();
    }

    public void SetMatch(int us, int them)
    {
        if (matchUs) matchUs.text = us.ToString();
        if (matchThem) matchThem.text = them.ToString();
    }

    public void FlashWinner(TeamId team)
    {
        Debug.Log($"[ScoreboardView_Text] Winner: {team}");
        // TODO: add UI highlight/animation if desired
    }
}
