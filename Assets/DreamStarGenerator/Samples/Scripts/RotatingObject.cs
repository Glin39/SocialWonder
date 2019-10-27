using UnityEngine;
using System.Collections;

namespace DreamStarGen.Algorithms
{
    public class RotatingObject : MonoBehaviour
    {
        public float RotateSpeed = 1;

        public float TargetSpeed = 50;
        public float LerpSpeed = 2f;
        public bool ShouldLerp = false;


        public bool X = false;
        public bool Y = false;
        public bool Z = true;

        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 rotator = new Vector3();
            if (X) rotator.x = RotateSpeed;
            if (Y) rotator.y = RotateSpeed;
            if (Z) rotator.z = RotateSpeed;
            GetComponentInChildren<Transform>().Rotate(rotator);

            if (ShouldLerp) RotateSpeed = Mathf.Lerp(RotateSpeed, TargetSpeed, LerpSpeed);

        }

        public void SetRotatingSpeed(float Value)
        {
            TargetSpeed = Value;
        }
    }
}