using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOver: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject HoverPanel;

    void Start()
    {
        HoverPanel.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverPanel.SetActive(false);
    }

}