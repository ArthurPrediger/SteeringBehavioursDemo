using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSwitch : MonoBehaviour
{
    private Image buttonImage;
    [SerializeField] private bool isOn = false;

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
        SetColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleButton()
    {
        isOn = !isOn;
        SetColor();
    }

    private void SetColor()
    {
        if (isOn)
        {
            buttonImage.color = Color.white;
        }
        else
        {
            buttonImage.color = Color.gray;
        }
    }
}
