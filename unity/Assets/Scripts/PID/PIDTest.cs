using System;
using UnityEngine;
using UnityEngine.UI;

public class PIDTest : MonoBehaviour
{
    public Text Agentangle;
    public Text Agentaction;

    Serial serial;

    [DebugGUIGraph(min: -90, max: 90, r: 1, g: 0, b: 0, autoScale: false)]
    float Angle;

    [DebugGUIGraph(min: -255, max: 255, r: 0, g: 1, b: 0, autoScale: false)]
    float Action;

    void Start()
    {
        serial = new Serial("COM8", 115200, true);
    }

    void Update()
    {
        string buffer = serial.receive();
        try
        {
            if (buffer != null)
            {
                string[] temp = buffer.Split('/');
                Angle = Convert.ToSingle(temp[0]) - 180.0f;
                Agentangle.text = Angle.ToString();
                Action = Convert.ToSingle(temp[1]);
                Agentaction.text = Action.ToString();
            }
        }
        catch (Exception e) { }
    }
}
