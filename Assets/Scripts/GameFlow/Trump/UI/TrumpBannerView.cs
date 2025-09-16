using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrumpBannerView : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text trumpText;         // e.g., "Trump: Hearts"
    public Image    trumpIcon;         // optional suit icon
    public TMP_Text dealerText;        // e.g., "Dealer: EAST"

    [Header("Icons (optional)")]
    public Sprite clubsIcon;
    public Sprite diamondsIcon;
    public Sprite heartsIcon;
    public Sprite spadesIcon;

    public void Show(Suit trump, SeatId dealer)
    {
        if (trumpText)  trumpText.text = $"Trump: {trump}";
        if (dealerText) dealerText.text = $"Dealer: {dealer.ToString().ToUpper()}";

        if (trumpIcon)
        {
            trumpIcon.enabled = true;
            trumpIcon.sprite = trump switch
            {
                Suit.Clubs    => clubsIcon,
                Suit.Diamonds => diamondsIcon,
                Suit.Hearts   => heartsIcon,
                Suit.Spades   => spadesIcon,
                _             => null
            };
        }

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
