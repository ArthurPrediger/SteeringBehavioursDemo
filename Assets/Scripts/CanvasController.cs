using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    private Canvas canvas;
    private Button[] buttons;
    private int curButtonActive = 0;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();

        buttons = gameObject.GetComponentsInChildren<Button>();

        for(int i = 0; i < buttons.Length; ++i)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => SetActiveButton(index));
            buttons[i].GetComponent<Image>().color = Color.gray;
        }

        if(buttons.Length > curButtonActive)
            buttons[curButtonActive].GetComponent<Image>().color = Color.white;

        canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            canvas.enabled = !canvas.enabled;
        }
    }

    public void SetActiveButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttons.Length)
            return;

        buttons[curButtonActive].GetComponent<Image>().color = Color.gray;
        buttons[buttonIndex].GetComponent<Image>().color = Color.white;
        curButtonActive = buttonIndex;
    }
}
