using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TriggerControl : MonoBehaviour
{
    public GameManager GM;
    public RectTransform parent;
    public RectTransform grid;
    public Image card;
    public GameObject APanel;
    
    public void Expand(RectTransform rect)
    {
        rect.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public void Shrink(RectTransform rect)
    {
        rect.localScale = new Vector3(1f, 1f, 1f);
    }

    public void Select(BaseEventData data)
    {
        data.selectedObject.transform.parent = parent.transform;
        data.selectedObject.GetComponent<RectTransform>().localPosition = new Vector3(0 ,0, 0);
        data.selectedObject.GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public void DeSelect(BaseEventData data)
    {
        data.selectedObject.transform.parent = grid.transform;
        data.selectedObject.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public void A(byte i)
    {
        GM.QuestionQ = i;
        GM.QuestionCardName = System.Convert.ToByte(card.sprite.name, 2);
        GM.OK_Question = true;
        APanel.SetActive(false);
    }

    public void PanelSelect()
    {
        GM.QuestionTarget = 6;
        APanel.SetActive(true);
    }

    public void Exit(BaseEventData data)
    {
        data.selectedObject.transform.parent.gameObject.SetActive(false);
    }

    public void Initiate()
    {
        
    }
}
