using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour {

    public int index;
    public Crosshair activeCrosshair;
    public Crosshair[] crosshairs;

    public Text pickItemText;

    public static CrosshairManager instace;
    public static CrosshairManager GetInstance()
    {
        return instace;
    }

    void Awake()
    {
        instace = this;
    }

    void Start()
    {
        for (int i = 0; i < crosshairs.Length; i++)
        {
            crosshairs[i].gameObject.SetActive(false);
        }

        crosshairs[index].gameObject.SetActive(true);
        activeCrosshair = crosshairs[index];
    }

    public void DefineCrosshairByIndex(int findIndex)
    {
        activeCrosshair = crosshairs[findIndex];
    }

    public void DefineCrosshairByName(string name)
    {
        for (int i = 0; i < crosshairs.Length; i++)
        {
            if(string.Equals(crosshairs[i].name,name))
            {
                activeCrosshair = crosshairs[i];
                break;
            }
        }
    }

}

