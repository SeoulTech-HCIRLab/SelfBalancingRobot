using MLAgents;
using UnityEngine;

public class RolyPoly : Agent
{
    public Rigidbody rolypoly;

    public float AngleVelocity;
    public float InitializeAngle;
    public float MaximumAngle;

    public override void AgentAction(float[] vectorAction)
    {
        float action = Mathf.Clamp(vectorAction[0], -1f, 1f);
        rolypoly.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        rolypoly.angularVelocity = new Vector3(0.0f, 0.0f, rolypoly.angularVelocity.z + action * AngleVelocity);

        //positive <- 0 -> negative (Check the z axis rotation direction simple version)
        float angle_z = rolypoly.rotation.eulerAngles.z;
        if (angle_z <= 360.0f && angle_z >= 270.0f) { angle_z -= 360.0f; }

        if (Mathf.Abs(angle_z) < MaximumAngle) { SetReward(1.0f); } else { Done(); }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float angle_z = rolypoly.rotation.eulerAngles.z;
        if (angle_z <= 360.0f && angle_z >= 270.0f) { angle_z -= 360.0f; }

        sensor.AddObservation(angle_z);
        sensor.AddObservation(rolypoly.angularVelocity.z);
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
