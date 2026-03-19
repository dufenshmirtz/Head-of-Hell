using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileCarouselSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text label;

    private List<string> options = new List<string>();
    private int currentIndex = 0;

    public event Action<int> OnIndexChanged;

    private void Awake()
    {
        leftButton.onClick.AddListener(Previous);
        rightButton.onClick.AddListener(Next);
    }

    public void SetOptions(List<string> newOptions)
    {
        options = newOptions ?? new List<string>();
        currentIndex = Mathf.Clamp(currentIndex, 0, options.Count - 1);
        RefreshUI();
    }

    public void SetIndexWithoutNotify(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, options.Count - 1);
        RefreshUI();
    }

    private void Next()
    {
        if (options.Count == 0) return;

        currentIndex++;
        if (currentIndex >= options.Count)
            currentIndex = 0;

        RefreshUI();
        OnIndexChanged?.Invoke(currentIndex);
    }

    private void Previous()
    {
        if (options.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = options.Count - 1;

        RefreshUI();
        OnIndexChanged?.Invoke(currentIndex);
    }
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    private void RefreshUI()
    {
        if (label != null && options.Count > 0)
            label.text = options[currentIndex];
    }
}