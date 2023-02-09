using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulListItem : MonoBehaviour
{
    public Transform presentTarget;
    public float presentIntervalX;// it must be + number because SoulList script will take absolute value.

    public int soulType; // 0-Normal,1-special,
    
    Vector3 nextPos;
    Vector3 offset;
    float initialIntervalX;

    private void Start()
    {
        initialIntervalX = Mathf.Abs(presentIntervalX);
    }

    private void FixedUpdate(){
        AdjustPos();
    }

    //***************************Method*****************************
    void AdjustPos() {
        // change intervalX if its target scale up; You can't keep seting presentintervalx but set one time
        if (presentTarget.localScale.x > 1 && Mathf.Abs(presentIntervalX) == initialIntervalX)
        {
            if (presentIntervalX < 0)
            {
                presentIntervalX = -(initialIntervalX + 0.1f);
            }
            if (presentIntervalX > 0)
            {
                presentIntervalX = initialIntervalX + 0.1f;
            }
        }
        else if(presentTarget.localScale.x <= 1 && Mathf.Abs(presentIntervalX) != initialIntervalX)
        {
            if (presentIntervalX < 0)
            {
                presentIntervalX = -initialIntervalX;
            }
            if (presentIntervalX > 0)
            {
                presentIntervalX = initialIntervalX;
            }
        }

        float soulSpeed = 8f;
        offset = new Vector3(presentIntervalX, 0, 0);
        
        nextPos = presentTarget.position - offset;// is at target's right
        transform.position= Vector3.Lerp(transform.position, nextPos, soulSpeed*Time.fixedDeltaTime);

    }
}
