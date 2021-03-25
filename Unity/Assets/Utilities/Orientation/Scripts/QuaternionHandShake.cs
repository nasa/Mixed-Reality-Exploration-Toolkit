// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class QuaternionHandShake : MonoBehaviour
{

    /// <summary>
    /// Conversion from left handed to right handed is achieved by swapping y and z and then taking the transpose
    /// </summary>

   
    public Quaternion leftQuat;

    public Vector3 lRow1;
    public Vector3 lRow2;
    public Vector3 lRow3;

    public Quaternion rightQuat;

    public Vector3 rRow1;
    public Vector3 rRow2;
    public Vector3 rRow3;

    private Quaternion compare;

    public Quaternion UnityQuat;
    public Quaternion RobotQuat;

    private bool solved = false;

    public void Start()
    {
        compare = leftQuat;
        UnityQuat = leftQuat;
        
    }

    public void Update()
    {
        if (!solved || !leftQuat.Equals(compare))
        {
            UnityQuat = leftQuat;
            RobotQuat = new Quaternion(UnityQuat.z, UnityQuat.y, UnityQuat.x, UnityQuat.w);

            float[,] lMatrix = QuatToMatrix(leftQuat);

            lRow1.Set(lMatrix[0, 0], lMatrix[0, 1], lMatrix[0, 2]);
            lRow2.Set(lMatrix[1, 0], lMatrix[1, 1], lMatrix[1, 2]);
            lRow3.Set(lMatrix[2, 0], lMatrix[2, 1], lMatrix[2, 2]);

            float[,] rMatrix = LeftToRightMatrix(lMatrix);

            rRow1.Set(rMatrix[0, 0], rMatrix[0, 1], rMatrix[0, 2]);
            rRow2.Set(rMatrix[1, 0], rMatrix[1, 1], rMatrix[1, 2]);
            rRow3.Set(rMatrix[2, 0], rMatrix[2, 1], rMatrix[2, 2]);

            rightQuat = MatrixtoQuat(rMatrix);

            solved = true;
            compare = leftQuat;
        }
    }

    private float[,] QuatToMatrix(Quaternion rotQuat)
    {
        // turn the rotQuat into a rotMatrix this is the simple part
        float[,] rotMatrix = new float[3, 3];

        // Grouping the caclulations makes it more efficient according to the interwebs
        //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.184.3942&rep=rep1&type=pdf

        float x2 = rotQuat.x + rotQuat.x;
        float y2 = rotQuat.y + rotQuat.y;
        float z2 = rotQuat.z + rotQuat.z;

        float xx2 = rotQuat.x * x2;
        float yy2 = rotQuat.y * y2;
        float zz2 = rotQuat.z * z2;

        rotMatrix[0, 0] = 1.0f - yy2 - zz2;
        rotMatrix[1, 1] = 1.0f - xx2 - zz2;
        rotMatrix[2, 2] = 1.0f - xx2 - yy2;

        float yz2 = rotQuat.y * z2;
        float wx2 = rotQuat.w * x2;

        rotMatrix[2, 1] = yz2 - wx2;
        rotMatrix[1, 2] = yz2 + wx2;

        float xy2 = rotQuat.x * y2;
        float wz2 = rotQuat.w * z2;

        rotMatrix[1, 0] = xy2 - wz2;
        rotMatrix[0, 1] = xy2 + wz2;

        float xz2 = rotQuat.x * z2;
        float wy2 = rotQuat.w * y2;

        rotMatrix[0, 2] = xz2 - wy2;
        rotMatrix[2, 0] = xz2 + wy2;

        return rotMatrix;
    }

    private Quaternion MatrixtoQuat(float[,] rotMatrix)
    {
        Quaternion rotQuat = new Quaternion(0, 0, 0, 0);

        if (rotMatrix[0, 0] + rotMatrix[1, 1] + rotMatrix[2, 2] > 0.0f)
        {
            float t = + rotMatrix[0, 0] + rotMatrix[1, 1] + rotMatrix[2, 2] + 1.0f;
            float s = 0.5f * (1 / Mathf.Sqrt(t));

            rotQuat.x = (rotMatrix[1, 2] - rotMatrix[2, 1]) * s;
            rotQuat.y = (rotMatrix[2, 0] - rotMatrix[0, 2]) * s;
            rotQuat.z = (rotMatrix[0, 1] - rotMatrix[1, 0]) * s;
            rotQuat.w = s * t;
        }

        else if (rotMatrix[0, 0] > rotMatrix[1, 1] && rotMatrix[0, 0] > rotMatrix[2, 2])
        {
            float t = +rotMatrix[0, 0] - rotMatrix[1, 1] - rotMatrix[2, 2] + 1.0f;
            float s = 0.5f * (1 / Mathf.Sqrt(t));

            rotQuat.x = s * t;
            rotQuat.y = (rotMatrix[0, 1] + rotMatrix[1, 0]) * s;
            rotQuat.z = (rotMatrix[2, 0] + rotMatrix[0, 2]) * s;
            rotQuat.w = (rotMatrix[1, 2] - rotMatrix[2, 1]) * s;
        }
        else if (rotMatrix[1, 1] > rotMatrix[2, 2])
        {
            float t = -rotMatrix[0, 0] + rotMatrix[1, 1] - rotMatrix[2, 2] + 1.0f;
            float s = 0.5f * (1 / Mathf.Sqrt(t));

            rotQuat.x = (rotMatrix[0, 1] + rotMatrix[1, 0]) * s;
            rotQuat.y = s * t;
            rotQuat.z = (rotMatrix[1, 2] + rotMatrix[2, 1]) * s;
            rotQuat.w = (rotMatrix[2, 0] - rotMatrix[0, 2]) * s;
        }
        else
        {
            float t = -rotMatrix[0, 0] - rotMatrix[1, 1] + rotMatrix[2, 2] + 1.0f;
            float s = 0.5f * (1 / Mathf.Sqrt(t));

            rotQuat.x = (rotMatrix[2, 0] + rotMatrix[0, 2]) * s;
            rotQuat.y = (rotMatrix[1, 2] + rotMatrix[2, 1]) * s;
            rotQuat.z = s*  t;
            rotQuat.w = (rotMatrix[0, 1] - rotMatrix[1, 0]) * s;
        }

        return rotQuat;
    }


    // these are the same function so I could just make then one LtoRtoLMatrix
    private float[,] LeftToRightMatrix(float[,] leftMatrix)
    {
        float[,] rightMatrix = new float[3, 3];

        float[,] holdMatrix = leftMatrix;

        rightMatrix[0, 0] = holdMatrix[0, 0];
        rightMatrix[1, 0] = holdMatrix[0, 2];
        rightMatrix[2, 0] = holdMatrix[0, 1];

        rightMatrix[0, 1] = holdMatrix[1, 0];
        rightMatrix[1, 1] = holdMatrix[1, 2];
        rightMatrix[2, 1] = holdMatrix[1, 1];

        rightMatrix[0, 2] = holdMatrix[2, 0];
        rightMatrix[1, 2] = holdMatrix[2, 2];
        rightMatrix[2, 2] = holdMatrix[2, 1];



        /* Transpose method
        rightMatrix[1, 0] = holdMatrix[0, 1];
        rightMatrix[0, 1] = holdMatrix[1, 0];
        rightMatrix[2, 0] = holdMatrix[0, 2];
        rightMatrix[0, 2] = holdMatrix[2, 0];
        rightMatrix[1, 2] = holdMatrix[2, 1];
        rightMatrix[2, 1] = holdMatrix[1, 2];

        rightMatrix[0, 0] = holdMatrix[0, 0];
        rightMatrix[1, 1] = holdMatrix[1, 1];
        rightMatrix[2, 2] = holdMatrix[2, 2];
        */


        return rightMatrix;
    }

    private float[,] RighttoLeftMatrix(float[,] rightMatrix)
    {
        float[,] leftMatrix = new float[3, 3];

        float[,] holdMatrix = rightMatrix;

        leftMatrix[1, 0] = holdMatrix[0, 1];
        leftMatrix[0, 1] = holdMatrix[1, 0];
        leftMatrix[2, 0] = holdMatrix[0, 2];
        leftMatrix[0, 2] = holdMatrix[2, 0];
        leftMatrix[1, 2] = holdMatrix[2, 1];
        leftMatrix[2, 1] = holdMatrix[1, 2];

        leftMatrix[0, 0] = holdMatrix[0, 0];
        leftMatrix[1, 1] = holdMatrix[1, 1];
        leftMatrix[2, 2] = holdMatrix[2, 2];



        return leftMatrix;
    }

}
