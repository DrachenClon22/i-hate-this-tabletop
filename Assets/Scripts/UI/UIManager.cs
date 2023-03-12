using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Color color;

    public GameObject fid;
    public TMPro.TextMeshProUGUI stepsText;
    private string _stepsText;

    private void Start()
    {
        _stepsText = stepsText.text;
    }
    private void Update()
    {
        stepsText.text = _stepsText.Replace("%steps%", PlayerController.currentSteps.ToString());
    }
    public void b_MakeGreen()
    {
        if (SelectHex.focus)
        {
            SelectHex.focus.GetComponent<Renderer>().materials[1].color = color;
            SelectHex.focus.GetComponent<Cell>().walkable = true;
        }
    }

    public void b_MakeBlue()
    {
        if (SelectHex.focus)
        {
            SelectHex.focus.GetComponent<Renderer>().materials[1].color = Color.red;
            SelectHex.focus.GetComponent<Cell>().walkable = false;
        }
    }

    public void b_MakeYellow()
    {
        
    }

    public void b_FindPath()
    {
        
    }

    public void b_DropTheDice()
    {
        PlayerController.DropTheDice(1,22);
    }
}
