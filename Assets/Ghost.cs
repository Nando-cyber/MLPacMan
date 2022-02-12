using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Ghost : Agent
{
    public float moveSpeed = 4f;
    public GameObject ghost;
    public bool pacManPowerMode;
    [SerializeField] private GameObject pacman;
    public Material powerMaterial;
    public Material defaultMaterial;
    private float distance;
    private bool forward = true;
    private bool left;
    private bool right;
    private bool back;
    public string area;
    private Vector3 direction;
    private Vector3 nextDirection;
    private bool decisionActive;
    public LayerMask wallLayer;
    public override void OnEpisodeBegin()
    {
            transform.localPosition =  new Vector3(0f, 2f, 3.5f);
            transform.Rotate(Vector3.zero);
            clearAll();
            forward = true;
            decisionActive = true;
            direction = Vector3.left;
         
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        distance = Vector3.Distance(pacman.transform.position, transform.position);
        //sensor.AddObservation(pacman.transform.localPosition);
        sensor.AddObservation(distance);
        if(distance > 2 && distance < 6)
        {
            AddReward(-0.005f*distance);
            
        }
        else if(distance <= 2) 
        {
            AddReward(+1.5f - (0.05f * distance));
        }
    }

    private bool occupied(Vector3 direction)
    {
        RaycastHit hitinfo;
        //bool hit = Physics.BoxCast(this.transform.localPosition, Vector3.one * 0.75f, direction, Quaternion.identity, 1.5f, wallLayer);
        bool hit = Physics.BoxCast(transform.position, Vector3.one * 0.49f, direction, out hitinfo, transform.rotation, 1f, wallLayer);
        return hit;
    }

    private void SetDirection(Vector3 direction)
    {
        if (!occupied(direction))
        {
            this.direction = direction;
            this.nextDirection = Vector3.zero;
        }
        else
        {
            this.nextDirection = direction;
        }
    }

    private void FixedUpdate()
    {
        if (this.nextDirection != Vector3.zero)
        {
            SetDirection(this.nextDirection);
        }

        Vector3 position = this.GetComponent<Rigidbody>().position;
        Vector3 translation = this.direction * this.moveSpeed * Time.deltaTime;
        ghost.transform.rotation = Quaternion.Slerp(ghost.transform.rotation, Quaternion.LookRotation(this.direction), Time.deltaTime * 40f);
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
    }

    private void clearAll()
    {
        forward = false;
        back = false;
        right = false;
        left = false;
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

    private void OnTriggerEnter(Collider other)
    {
        
        /*if (other.tag == "turning" || other.tag == "turningGhost")
        {
            
            decisionActive = true;
            transform.localPosition = new Vector3(other.transform.localPosition.x, transform.localPosition.y, other.transform.localPosition.z);
            // transform.Translate(new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z) * Time.deltaTime);
            foreach (Transform p in other.gameObject.transform)
            {

                if (p.tag == "Forward")
                {
                    forward = true;
                }
                if (p.tag == "Right")
                {
                    right = true;
                }
                if (p.tag == "Left")
                {
                    left = true;
                }
                if (p.tag == "Backward")
                {
                    back = true;
                }
            }

        }*/
        if(other.tag == "PacMan" && !pacManPowerMode)
        {
            AddReward(22f);
        }
        else if(other.tag == "PacMan" && pacManPowerMode)
        {
            AddReward(-10f);
        }

        if(other.tag == "TopLeft" || other.tag == "TopRight" || other.tag == "BottRight" || other.tag == "BottLeft" || other.tag == "Center")
        {
            area = other.tag;
        }
    }

    public void setPowerMode(bool power)
    {
        pacManPowerMode = power;
        if(power)
        {
            ghost.GetComponent<MeshRenderer>().material = powerMaterial;   
        }
        else
        {
            ghost.GetComponent<MeshRenderer>().material = defaultMaterial;
        }
    }
}

