using UnityEngine;

public class MinionMoverMarker : MonoBehaviour
{
    public float markertimer;
    public SpriteRenderer mysprite;

    private void Start(){
        //mysprite.enabled= false;
    }

    private void Update()
    {
        if (markertimer >= 0 && !Input.GetMouseButton(0)) markertimer -= Time.deltaTime;

        if (markertimer <= 0) {
            transform.parent = null;
            mysprite.enabled = false; }
    }

    public void relocateMarker(Vector3 pos, Transform subject)
    {
        float lastTime = 1.5f;

        mysprite.enabled = true;

        // don't hit object
        if (subject == null)
        {
            transform.position = pos;
            transform.parent= null;
        }
        // hit object
        else
        {
            GameObject newMarker = Instantiate(gameObject, subject.position, subject.rotation, subject);
            newMarker.GetComponent<MinionMoverMarker>().markertimer = lastTime;
            mysprite.enabled = false;
            Destroy(newMarker,lastTime);
        }

        markertimer = lastTime;
    }

    public void HideMarker()
    {
        mysprite.enabled = false;
    }
}
