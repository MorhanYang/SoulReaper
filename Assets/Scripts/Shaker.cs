using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    //Attach this to a combat unit so its sprite shakes.
    [SerializeField]
    private Transform mySprite;
    //[SerializeField]
    private Vector3 initialLocalPos;
    //[SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private float returnStrengh;
    [SerializeField]
    public float mass = 1;
    [SerializeField]
    float knockBack;

    void Start()
    {
        initialLocalPos = mySprite.localPosition;
    }

    void Update()
    {
        //Debug
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            AddImpact(new Vector3(1, 0, 0), 20, false);
        }

        if (velocity != Vector3.zero)
        {
            //Change Velocity as a spring pulls the sprite back to center
            Vector3 returnFoce = (initialLocalPos - mySprite.localPosition).normalized * returnStrengh * 0.1f;
            Vector3 Acceleration = returnFoce / mass;
            velocity += Acceleration;

            //Simplified friction that eventually slows the spirte down
            velocity *= (1 - Time.deltaTime * 25);

            //Static friction
            if (velocity.magnitude<0.1f && Vector3.Distance(mySprite.localPosition,initialLocalPos) < 0.1f)
            {
                velocity *= 0;
                mySprite.localPosition = initialLocalPos;
            }

            mySprite.position += velocity * Time.deltaTime;
        }
    }

    public void AddImpact(Vector3 direction, float Force, bool includeYaxis) 
    {
        Vector3 acceleration = direction.normalized * Force / mass;
        if (!includeYaxis) 
        {
            acceleration = new Vector3(acceleration.x, 0, acceleration.z);
        }
        velocity += acceleration;

        // knock back
        transform.position += direction.normalized * (knockBack / mass) * 0.05f;
    }
}
