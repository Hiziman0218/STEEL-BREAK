using RaycastPro.Detectors;

namespace Plugins.RaycastPro.Demo.Scripts
{
    using global::RaycastPro.Detectors2D;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class SteeringController : MonoBehaviour
    {
        private Rigidbody rb;
        public SteeringDetector detector;

        public float accelerationSpeed = 4f;
        public float arriveDistance = 2f;
        public float turnRate = 15;

        [SerializeField] private bool movable = true;
        [SerializeField] private bool rotatable = true;
        [SerializeField] private bool fixRotate = true;

        [Range(2f, 50)] public float speed = 10;

        // ’âŽ~”»’è
        public bool isStopped { get; private set; }
        // Žc‚è‹——£
        public float remainingDistance { get; private set; }



        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Update()
        {

            // Žc‚è‹——£‚ðŒvŽZ
            remainingDistance = Vector3.Distance(transform.position, detector.destination.position);

            // ’âŽ~”»’è
            isStopped = remainingDistance <= detector.stoppingDistance;


            if (movable)
            {
                var inArriveDistance = Vector3.Distance(transform.position, detector.destination.position) < arriveDistance;
                
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, inArriveDistance ? Vector3.zero : detector.SteeringDirection * speed, 
                    1 - Mathf.Exp(-accelerationSpeed * Time.deltaTime));
            }

            if (rotatable)
            {
                if (fixRotate)
                {
                    transform.forward = Vector3.ProjectOnPlane(detector.SteeringDirection, Vector3.up);
                }
                else
                {
                    rb.rotation = Quaternion.Lerp(rb.rotation,
                        Quaternion.LookRotation(detector.SteeringDirection == Vector3.zero ? transform.forward : detector.SteeringDirection, Vector3.up), 1 - Mathf.Exp(-turnRate * Time.deltaTime));
                }
            }
        }
    }
}