using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SevenGame.Utility {
    
    public class FollowSpline : MonoBehaviour{

        [SerializeField] private float maxSpeed;
        [SerializeField] private float moveSpeed;
        [SerializeField] private MovementDirection movementDirection;

        [SerializeField] private Spline spline;
        private OrientedPoint splinePosition;
        [Range(0, 1f)] [SerializeField] private float weight = 0.5f;
        [Range(0, 1f)] [SerializeField] private float t;

        private float maxSpeedOnSlowDown;


        private bool goingForward => movementDirection == MovementDirection.Forward;



        void FixedUpdate(){
            if (movementDirection == MovementDirection.None) {
                maxSpeedOnSlowDown = 0f;
                return;
            }


            float direction = (float)movementDirection;

            bool stoppingPointForward = spline.hasStoppingPoint && ((spline.stoppingPoint < t && movementDirection == MovementDirection.Backward) || (spline.stoppingPoint > t && movementDirection == MovementDirection.Forward));
            bool endOfTheLine = goingForward ? spline.nextSpline == null : spline.previousSpline == null;

            if ( stoppingPointForward || endOfTheLine ) {

                // When at the end of the line or at the stopping point, slow down
                float stoppingPoint = stoppingPointForward ? spline.stoppingPoint : (goingForward ? 1f : 0f);

                float slowT = goingForward ? t / stoppingPoint : (1f - t) / (1f - stoppingPoint);
                
                moveSpeed = Mathf.Lerp(maxSpeedOnSlowDown, 0f, slowT);



                bool reachedStoppingPoint = Mathf.Abs(t - stoppingPoint) < 0.01f;

                if (reachedStoppingPoint) {
                    if (stoppingPointForward) {
                        // If at the stopping point, slow down and stop, then continue
                        t = stoppingPoint;
                    } else {
                        // If at the end of the line, stop and turn around
                        movementDirection = (MovementDirection)(-(int)movementDirection);
                    }
                }

            } else {

                // moveSpeed = Mathf.MoveTowards(moveSpeed, maxSpeed, (1f - weight) * GameUtility.timeDelta);
                moveSpeed = maxSpeed;
                maxSpeedOnSlowDown = moveSpeed;
                
            }

            // Move along spline
            float distanceToMove = (moveSpeed * direction) / 40f * GameUtility.timeDelta;
            t += distanceToMove;


            // If the object has reached the end of the spline, go to the next one
            while ( goingForward && t > 1f && spline.nextSpline != null) {
                spline = spline.nextSpline;
                t -= 1f;
            }
            while ( !goingForward && t < 0f && spline.previousSpline != null) {
                spline = spline.previousSpline;
                t += 1f;
            }
 
            
            // Move
            splinePosition = spline.GetPointUniform(t);

            transform.position = Vector3.Lerp(transform.position, splinePosition.position, GameUtility.timeDelta);
            transform.rotation = Quaternion.Slerp(transform.rotation, splinePosition.rotation, GameUtility.timeDelta);
        }



        private enum MovementDirection {
            Backward = -1,
            None = 0,
            Forward = 1
        }

    }
}