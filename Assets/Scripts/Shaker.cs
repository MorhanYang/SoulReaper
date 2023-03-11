using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    //Attach this to a combat unit so its sprite shakes.
    [SerializeField]
    private Transform MySprite;
    //[SerializeField]
    private Vector3 InitialLocalPos;
    //[SerializeField]
    private Vector3 Velocity;
    [SerializeField]
    private float SpringStrengh;
    [SerializeField]
    public float Mass;

    void Start()
    {
        InitialLocalPos = MySprite.localPosition;
    }

    void Update()
    {
        //Debug
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            AddImpact(new Vector3(1, 0, 0), 5, false);
        }

        //Change Velocity as a spring pulls the sprite back to center
        Vector3 SpringForce = (InitialLocalPos - MySprite.localPosition) * SpringStrengh;
        Vector3 Acceleration = SpringForce / Mass;
        Velocity += Acceleration;

        //Simplified friction that eventually slows the spirte down
        Velocity *= (1 - Time.deltaTime * 10);

        //Static friction
        if (Velocity.magnitude < 0.05f && Vector3.Distance(MySprite.localPosition, InitialLocalPos) < 0.05f)
        {
            Velocity *= 0;
            MySprite.localPosition = InitialLocalPos;
        }

        MySprite.position += Velocity * Time.deltaTime;
    }

    public void AddImpact(Vector3 direction, float Force, bool includeYaxis) 
    {
        Vector3 acceleration = direction.normalized * Force / Mass;
        if (!includeYaxis) 
        {
            acceleration = new Vector3(acceleration.x, 0, acceleration.z);
        }
        Velocity += acceleration;
    }
}
