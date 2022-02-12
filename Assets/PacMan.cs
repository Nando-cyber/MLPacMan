using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PacMan : Agent
{
    [SerializeField] private GameObject centerPills;
    [SerializeField] private GameObject pacman;
    [SerializeField] private GameObject topRight;
    [SerializeField] private GameObject topLeft;
    [SerializeField] private GameObject bottRight;
    [SerializeField] private GameObject bottLeft;
    [SerializeField] private GameObject red;
    [SerializeField] private GameObject blue;
    public LayerMask wallLayer;
    public Material targetMat;
    public Material defPillMat;
    private float moveSpeed = 4f;
    private bool decisionActive;
    private bool forward;
    private bool left = true;
    private bool right = true;
    private bool back;
    private Vector3 direction;
    private Vector3 nextDirection;
    private int pillnum;
    private int current;
    public int lives;
    private int lost = 0;
    private int win = 0;
    private bool powerMode;
    private float startT;
    private string area;
    private string targetArea;
    private bool stay;
    private bool reaching;
    private bool lastZone;
    private int prevPillNum;
    private Transform targetPill;
    private bool collided;

    public override void OnEpisodeBegin()
    {
        pillnum = 0;
        targetArea = "BottLeft";
        foreach (Transform pill in centerPills.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
            pill.gameObject.SetActive(true);
            pillnum++;
        }

        foreach (Transform pill in topLeft.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
            pill.gameObject.SetActive(true);
            pillnum++;
        }

        foreach (Transform pill in topRight.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
            pill.gameObject.SetActive(true);
            pillnum++;
        }

        foreach (Transform pill in bottLeft.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
            pill.gameObject.SetActive(true);
            pillnum++;
        }

        foreach (Transform pill in bottRight.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
            pill.gameObject.SetActive(true);
            pillnum++;
        }
        
        transform.localPosition = new Vector3(0, 2, -4.5f);
        clearAll();
        decisionActive = true;
        right = true;
        left = true;
        lives = 3;
        lastZone = false;
        prevPillNum = pillnum;
        current = pillnum;
        this.nextDirection = Vector3.zero;
        this.direction = Vector3.left;
    }

    private bool occupied(Vector3 direction)
    {
        RaycastHit hitinfo;
        //bool hit = Physics.BoxCast(this.transform.localPosition, Vector3.one * 0.75f, direction, Quaternion.identity, 1.5f, wallLayer);
        bool hit = Physics.BoxCast(transform.position, Vector3.one * 0.49f, direction, out hitinfo, transform.rotation, 1f, wallLayer);
        return hit;
    }

    private void OnDrawGizmos()
    {
        RaycastHit hit;
        bool ishit = Physics.BoxCast(transform.position, Vector3.one * 0.49f, this.nextDirection, out hit, transform.rotation, 1f, wallLayer);
        if (ishit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.left * hit.distance);
            Gizmos.DrawWireCube(transform.position + this.nextDirection * hit.distance, 2 * Vector3.one * 0.49f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, this.nextDirection * 1.5f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 3;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 0;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 2;
        }
    }

    private void SetDirection(Vector3 direction)
    {
        if (!occupied(direction))
        {
            this.direction = direction;
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.nextDirection = Vector3.zero;
        }
        else
        {
            this.nextDirection = direction;
        }
    }

    private bool checkDirection(Vector3 direction)
    {
        RaycastHit hitinfo;
        bool hit = Physics.BoxCast(transform.position, Vector3.one * 0.48f, direction, out hitinfo, transform.rotation, 1.5f, wallLayer);
        Debug.Log(hitinfo.distance);
        if (hitinfo.distance > 0.251 && hitinfo.distance < 0.249)
        {
            return true;
        }
        else return false;
    }

    private void FixedUpdate()
    {
        if (this.nextDirection != Vector3.zero)
        {
            SetDirection(this.nextDirection);
        }

        Vector3 position = this.GetComponent<Rigidbody>().position;
        Vector3 translation = this.direction * this.moveSpeed * Time.deltaTime;
        pacman.transform.rotation = Quaternion.Slerp(pacman.transform.rotation, Quaternion.LookRotation(this.direction), Time.deltaTime * 40f);
        this.GetComponent<Rigidbody>().position = (position + translation);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        if (discreteActions[0] == 0)
        {
            this.nextDirection = Vector3.forward;
        }
        if (discreteActions[0] == 1)
        {
            this.nextDirection = Vector3.back;
        }
        if (discreteActions[0] == 2)
        {
            this.nextDirection = Vector3.right;
        }
        if (discreteActions[0] == 3)
        {
            this.nextDirection = Vector3.left;
        }

        AddReward(-0.0005f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        reassign();
        sensor.AddObservation(transform.position);
        int count = 0;
        red.TryGetComponent<Ghost>(out Ghost g1);
        //blue.TryGetComponent<Ghost>(out Ghost g2);
        
        /*if (!isClearArea("TopRight") && arePillsEmpty("TopLeft") && arePillsEmpty("Center") && arePillsEmpty("BottRight") && arePillsEmpty("BottLeft"))
        {
            lastZone = true;
            targetArea = "TopRight";
        }
        else if (!isClearArea("TopLeft") && arePillsEmpty("TopRight") && arePillsEmpty("Center") && arePillsEmpty("BottRight") && arePillsEmpty("BottLeft"))
        {
            lastZone = true;
            targetArea = "TopLeft";
        }
        else if (!isClearArea("Center") && arePillsEmpty("TopLeft") && arePillsEmpty("TopRight") && arePillsEmpty("BottRight") && arePillsEmpty("BottLeft"))
        {
            lastZone = true;
            targetArea = "Center";
        }
        else if (!isClearArea("BottRight") && arePillsEmpty("TopLeft") && arePillsEmpty("Center") && arePillsEmpty("TopRight") && arePillsEmpty("BottLeft"))
        {
            lastZone = true;
            targetArea = "BottRight";
        }
        else if (!isClearArea("BottLeft") && arePillsEmpty("TopLeft") && arePillsEmpty("Center") && arePillsEmpty("BottRight") && arePillsEmpty("TopRight"))
        {
            lastZone = true;
            targetArea = "BottLeft";
        }

        if ((area == targetArea && isClearArea(targetArea) && !arePillsEmpty(targetArea)) || lastZone)
        {
            stay = true;
        }
        else if ((!isClearArea(targetArea) || arePillsEmpty(targetArea)) && !lastZone) // Se l'area non è libera oppure sono finite le pills
        {
            stay = false;
        }
        if (!reaching && !stay && !lastZone) // Normale caso in cui PacMan deve cambiare zona
        {
            changeTargetArea();
            reaching = true;
        }
        if (reaching && !isClearArea(targetArea) && !lastZone) // Caso in cui PacMan sta raggiungendo la zona ed entra un ghost
        {
            changeTargetArea();
        }*/
        
        if (area == "BottLeft" && !arePillsEmpty(area))
        {
            foreach (Transform pill in bottLeft.transform)
            {
                if (pill.gameObject.activeSelf  )
                {               
                    targetPill = pill;
                    sensor.AddObservation(pill.gameObject.transform.localPosition);
                    float distance = Vector3.Distance(pill.gameObject.transform.localPosition, transform.localPosition);
                    sensor.AddObservation(distance);
                    pill.GetComponent<MeshRenderer>().material = targetMat;
                }
            }
        }else
        {
            changeTargetArea();
        }

        if (area == "TopRight" && !arePillsEmpty(area))
        {
            foreach (Transform pill in topRight.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    targetPill = pill;
                    sensor.AddObservation(pill.gameObject.transform.localPosition);
                    float distance = Vector3.Distance(pill.gameObject.transform.localPosition, transform.localPosition);
                    sensor.AddObservation(distance);
                    pill.GetComponent<MeshRenderer>().material = targetMat;
                }
            }
        }
        else
        {
            changeTargetArea();
        }
        if (area == "Center" && !arePillsEmpty(area))
        {
            foreach (Transform pill in centerPills.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    targetPill = pill;
                    sensor.AddObservation(pill.gameObject.transform.localPosition);
                    float distance = Vector3.Distance(pill.gameObject.transform.localPosition, transform.localPosition);
                    sensor.AddObservation(distance);
                    pill.GetComponent<MeshRenderer>().material = targetMat;
                }
            }
        }
        else
        {
            changeTargetArea();
        }

        if (area == "BottRight" && !arePillsEmpty(area))
        {
            foreach (Transform pill in bottRight.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    targetPill = pill;
                    sensor.AddObservation(pill.gameObject.transform.localPosition);
                    float distance = Vector3.Distance(pill.gameObject.transform.localPosition, transform.localPosition);
                    sensor.AddObservation(distance);
                    pill.GetComponent<MeshRenderer>().material = targetMat;
                }
            }
        }


        if (area == "TopLeft" && !arePillsEmpty(area))
        {
            foreach (Transform pill in topLeft.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    targetPill = pill;
                    sensor.AddObservation(pill.gameObject.transform.localPosition);
                    count++;
                    float distance = Vector3.Distance(pill.gameObject.transform.localPosition, transform.localPosition);
                    sensor.AddObservation(distance);
                    pill.GetComponent<MeshRenderer>().material = targetMat;
                }
            }
        }
        else
        {
            changeTargetArea();
        }

        // Direzione della target pill
        //sensor.AddObservation((targetPill.localPosition - transform.localPosition).normalized);
        // Direzione PacMan
        //sensor.AddObservation(this.direction);

        sensor.AddObservation(red.transform.position);
        // sensor.AddObservation(blue.transform.localPosition);
        float redDist = Vector3.Distance(red.transform.localPosition, transform.localPosition);
        //float blueDist = Vector3.Distance(blue.transform.localPosition, transform.localPosition);
        if (redDist < 4)
        {
            AddReward(-0.005f * redDist);
        }
        /*
        if(blueDist < 5)
        {
            AddReward(-0.005f * blueDist);
        } */
        sensor.AddObservation(redDist);
        // sensor.AddObservation(blueDist);
        if (powerMode && (Time.time - startT) > 15)
        {
            powerMode = false;
            red.TryGetComponent<Ghost>(out Ghost r);
            r.setPowerMode(false);
            //blue.TryGetComponent<Ghost>(out Ghost b);
           // b.setPowerMode(false);
        }
        sensor.AddObservation(pillnum);
        sensor.AddObservation(current);
        sensor.AddObservation(powerMode);
        AddReward(-0.0001f);
        if(current == pillnum)
        {
            Debug.Log("No balls, lose point;");
            AddReward(-0.005f);
        }
        else
        {
            current = pillnum;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            collided = true;
        }
        if (collision.gameObject.tag == "Ghost")
        {
            if (powerMode)
            {
                AddReward(+1);
                collision.gameObject.TryGetComponent<Ghost>(out Ghost g);
                g.EndEpisode();
            }

            else if (lives == 0)
            {
                AddReward(-22f);
                lost++;
                TextMesh t = GameObject.Find("Lost").GetComponent<TextMesh>();
                int current = Convert.ToInt32(t.text.ToString());
                t.text = "" + (current + 1);
                red.TryGetComponent<Ghost>(out Ghost r);
                r.EndEpisode();
                //blue.TryGetComponent<Ghost>(out Ghost b);
               // b.EndEpisode();
                EndEpisode();
            }
            else
            {
                lives--;
                AddReward(-10f);
                reposition();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pill"))
        {
            GameObject pill = other.gameObject;
            pill.gameObject.SetActive(false);

            pillnum--;            
            AddReward(1 + ((prevPillNum - pillnum)*0.2f));
            checkVictory();
        }
        else if (other.CompareTag("PowerPill"))
        {
            pillnum--;
            AddReward(1 + ((prevPillNum - pillnum)*0.2f));
            GameObject pill = other.gameObject;
            pill.gameObject.SetActive(false);
            checkVictory();
            powerMode = true;
            //AddReward(+0.5f);
            red.TryGetComponent<Ghost>(out Ghost r);
            r.setPowerMode(true);
            //blue.TryGetComponent<Ghost>(out Ghost b);
            //b.setPowerMode(true);
            startT = Time.time;
        }

        if (other.tag == "TopLeft" || other.tag == "TopRight" || other.tag == "BottRight" || other.tag == "BottLeft" || other.tag == "Center")
        {
            area = other.tag;
            if (other.tag == targetArea)
            {
                reaching = false;
            }
        }
    }

    private void clearAll()
    {
        forward = false;
        back = false;
        right = false;
        left = false;
    }

    private void reposition()
    {
        transform.localPosition = new Vector3(0, 2, -4.5f);
        clearAll();
        decisionActive = true;
        right = true;
        left = true;
    }

    private void reassign()
    {
        foreach (Transform pill in centerPills.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
        }

        foreach (Transform pill in topLeft.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
        }

        foreach (Transform pill in topRight.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
        }

        foreach (Transform pill in bottLeft.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
        }

        foreach (Transform pill in bottRight.transform)
        {
            pill.GetComponent<MeshRenderer>().material = defPillMat;
        }
    }

    private bool isClearArea(string a)
    {
        red.TryGetComponent<Ghost>(out Ghost g1);
        //blue.TryGetComponent<Ghost>(out Ghost g2);  || a == g2.area
        if (a == g1.area )
        {
            return false;
        }
        else return true;
    }

    private bool arePillsEmpty(string a)
    {
        if (a == "TopRight")
        {
            foreach (Transform pill in topRight.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
        else if (a == "TopLeft")
        {
            foreach (Transform pill in topLeft.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
        else if (a == "Center")
        {
            foreach (Transform pill in centerPills.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
        else if (a == "BottRight")
        {
            foreach (Transform pill in bottRight.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            foreach (Transform pill in bottLeft.transform)
            {
                if (pill.gameObject.activeSelf)
                {
                    return false;
                }
            }
            return true;
        }
    }

    private void changeTargetArea()
    {
        if (area == "BottLeft")
        {
            if (isClearArea("TopRight") && !arePillsEmpty("TopRight"))
            {
                targetArea = "TopRight";
            }
            else if (isClearArea("Center") && !arePillsEmpty("Center"))
            {
                targetArea = "Center";
            }
            else if (isClearArea("BottRight") && !arePillsEmpty("BottRight"))
            {
                targetArea = "BottRight";
            }
            else if (isClearArea("TopLeft") && !arePillsEmpty("TopLeft"))
            {
                targetArea = "TopLeft";
            }
        }
        if (area == "TopRight")
        {
            if (isClearArea("Center") && !arePillsEmpty("Center"))
            {
                targetArea = "Center";
            }
            else if (isClearArea("BottRight") && !arePillsEmpty("BottRight"))
            {
                targetArea = "BottRight";
            }
            else if (isClearArea("TopLeft") && arePillsEmpty("TopLeft"))
            {
                targetArea = "TopLeft";
            }
            else if (isClearArea("BottLeft") && !arePillsEmpty("BottLeft"))
            {
                targetArea = "BottLeft";
            }
        }
        if (area == "Center")
        {
            if (isClearArea("TopLeft") && !arePillsEmpty("TopLeft"))
            {
                targetArea = "TopLeft";
            }
            else if (isClearArea("TopRight") && !arePillsEmpty("TopRight"))
            {
                targetArea = "TopRight";
            }
            else if (isClearArea("BottRight") && !arePillsEmpty("BottRight"))
            {
                targetArea = "BottRight";
            }
            else if (isClearArea("BottLeft") && !arePillsEmpty("BottLeft"))
            {
                targetArea = "BottLeft";
            }
        }
        if (area == "BottRight")
        {
            if (isClearArea("TopLeft") && !arePillsEmpty("TopLeft"))
            {
                targetArea = "TopLeft";
            }
            else if (isClearArea("TopRight") && !arePillsEmpty("TopRight"))
            {
                targetArea = "TopRight";
            }
            else if (isClearArea("Center") && !arePillsEmpty("Center"))
            {
                targetArea = "Center";
            }
            else if (isClearArea("BottLeft") && !arePillsEmpty("BottLeft"))
            {
                targetArea = "BottLeft";
            }
        }
        if (area == "TopLeft")
        {
            if (isClearArea("BottRight") && !arePillsEmpty("BottRight"))
            {
                targetArea = "BottRight";
            }
            else if (isClearArea("BottLeft") && !arePillsEmpty("BottLeft"))
            {
                targetArea = "BottLeft";
            }
            else if (isClearArea("TopRight") && !arePillsEmpty("TopRight"))
            {
                targetArea = "TopRight";
            }
            else if (isClearArea("Center") && !arePillsEmpty("Center"))
            {
                targetArea = "Center";
            }
        }
    }

    private void checkVictory()
    {
        if (pillnum == 0)
        {
            AddReward(44f);
            Ghost g1 = red.GetComponent<Ghost>();
            g1.EndEpisode();
            //Ghost g2 = blue.GetComponent<Ghost>();
            //g2.EndEpisode();
            win++;
            TextMesh t = GameObject.Find("Score").GetComponent<TextMesh>();
            int c = Convert.ToInt32(t.text.ToString());
            t.text = "" + (c + 1);
            EndEpisode();
        }
    }

}


