using System;
using UnityEngine;

public interface IPlayerMode
{
    public Vector3 SimulateMovement(float dirX, float dirZ, float moveSpeed, float acceleration, float velPower, float cameraAngle,
                             float playerAngle, float turnSmoothTime, ref float turnSmoothVelocity, ref float rotateAngle, Vector3 velocity);
}
