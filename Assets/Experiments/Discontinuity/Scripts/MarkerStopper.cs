using UnityEngine;
using System.IO.Ports;
using System;


public class MarkerStopper : MonoBehaviour
{
    /**
     * Open a connection to the arduino. It will send a 0 if the button is pressed and
     * a 1 when it is released.
     */
    SerialPort stream = new SerialPort("COM5", 9600);

    public PropDriftController driftController;

    public bool aux = true;

    // Use this for initialization
    void Start()
    {
        stream.ReadTimeout = 1;
        stream.Open();
    }


    // Update is called once per frame
    void Update() {
        try {
            string b = stream.ReadLine();
            b = b.Trim();
            Debug.Log(b);
            if (b == "0" && aux) {
                Debug.Log("Button pressed");
                driftController.HandleEvent(DriftEvents.ButtonPressed);
                aux = false;
            } else if (b == "1" && !aux) {
                aux = true;
            }
        }
        catch
        {
        }
    }
}
