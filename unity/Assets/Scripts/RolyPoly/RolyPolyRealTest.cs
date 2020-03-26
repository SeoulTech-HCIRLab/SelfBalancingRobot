using MLAgents;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RolyPolyRealTest : Agent
{
    public Text Agentangle;
    public Text Agentaction;

    public float Mortor_Sensitivity = 1.0f;
    public float Gyro_Sensitivity = 1.0f;
    public float angleoffset = 0.0f;
    public float actionoffset = 0.0f;
    public float angularvelocityscale = 2.0f;

    float angle = 0.0f;
    float angularvelocity = 0.0f;

    Serial serial;

    [DebugGUIGraph(min: -90, max: 90, r: 1, g: 0, b: 0, autoScale: false)]
    float Angle;

    [DebugGUIGraph(min: -255, max: 255, r: 0, g: 1, b: 0, autoScale: false)]
    float Action;

    UnscentedKalmanFilter anglefilter;
    UnscentedKalmanFilter actionfilter;

    private void Start()
    {
        //please check your arduino port
        serial = new Serial("COM8", 115200, true);
        anglefilter = new UnscentedKalmanFilter();
        actionfilter = new UnscentedKalmanFilter();
    }

    private void OnApplicationQuit()
    {
        serial.CloseSerial();
    }

    public override void AgentAction(float[] vectorAction)
    {
        float action = Mathf.Clamp(vectorAction[0], -1f, 1f);
        action = Mathf.Round(Mathf.Clamp(action * 254.0f * Mortor_Sensitivity + actionoffset, -254.0f, 254.0f));
        
        //var measurement = new double[] { action };
        //actionfilter.Update(measurement);
        //action = Mathf.Clamp(Mathf.Round((float)actionfilter.getState()[0] * 254.0f * Mortor_Sensitivity) + actionoffset, -254.0f, 254.0f);

        Action = action;
        serial.send(action.ToString());
        Agentaction.text = "Agent Action : " + action.ToString();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //               from arduino           from unity
        //protocol => angle/angularvelocity       action
        string buffer = serial.receive();
        try
        {
            if (buffer != null)
            {
                string[] commands = buffer.Split('/');
                angularvelocity = (float)Math.Round(Convert.ToSingle(commands[0]), 3) / angularvelocityscale;
                angle = (float)Math.Round(Gyro_Sensitivity * Convert.ToSingle(commands[1]) + angleoffset, 2);
            }
        }
        catch (Exception e) { Debug.Log("Observation Fail !!"); }

        Agentangle.text = "Agent Angle : " + angle.ToString();

        var anglemeasurement = new double[] { angle };
        anglefilter.Update(anglemeasurement);
        angle = (float)anglefilter.getState()[0];

        Angle = angle;
        sensor.AddObservation(angle);
        sensor.AddObservation(angularvelocity);
        //Debug.Log(angularvelocity);
    }
}
