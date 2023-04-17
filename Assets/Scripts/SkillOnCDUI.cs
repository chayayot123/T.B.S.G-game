using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillOnCDUI : MonoBehaviour
{
    public GameObject popupPanel;

    void Start()
    {
        popupPanel.SetActive(false);
    }

    public void SkillOnCDPopUp()
    {
        popupPanel.SetActive(true);
        StartCoroutine(HidePopupAfterSeconds(1));
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    private IEnumerator HidePopupAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HidePopup();
    }
}
