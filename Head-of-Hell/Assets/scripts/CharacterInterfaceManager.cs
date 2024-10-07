using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInterfaceManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Image cdbarimage;
    public Sprite activeSprite, ogSprite;
    public Slider cooldownSlider;
    public AudioManager audioManager;
    public helthbarscript healthbar;
    public TextMeshProUGUI P1Name, winner;
    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    public GameObject quickAttackIndicator;
}
