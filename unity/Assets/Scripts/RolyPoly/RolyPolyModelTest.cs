using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RolyPolyModelTest : Agent
{
    public Text Agentangle;
    public Text Agentaction;

    public Rigidbody rolypoly;

    public float AngleVelocity;
    public float InitializeAngle;
    public float MaximumAngle;

    [DebugGUIGraph(min: -90, max: 90, r: 1, g: 0, b: 0, autoScale: false)]
    float Angle;

    [DebugGUIGraph(min: -255, max: 255, r: 0, g: 1, b: 0, autoScale: false)]
    float Action;

    public override void AgentAction(float[] vectorAction)
    {
        float action = Mathf.Clamp(vectorAction[0], -1f, 1f);
        rolypoly.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        rolypoly.angularVelocity = new Vector3(0.0f, 0.0f, rolypoly.angularVelocity.z + action * AngleVelocity);

        Action = action * 254.0f;
        Agentaction.text = "Agent Action : " + (action * 254.0f).ToString();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float angle_z = rolypoly.rotation.eulerAngles.z;
        if (angle_z <= 360.0f && angle_z >= 270.0f) { angle_z -= 360.0f; }

        sensor.AddObservation(angle_z);
        sensor.AddObservation(rolypoly.angularVelocity.z);
        Angle = angle_z;
        Agentangle.text = "Agent Angle : " + angle_z.ToString();
        Debug.Log(rolypoly.angularVelocity.z);
    }

    public override float[] Heuristic()
    {
        var action = new float[1];
        action[0] = Input.GetAxis("Horizontal");
        return action;
    }

    public override void AgentReset()
    {
        rolypoly.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        rolypoly.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        rolypoly.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        rolypoly.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(-InitializeAngle, InitializeAngle));
    }
}
