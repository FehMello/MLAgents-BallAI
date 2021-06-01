using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Agente : Agent
{
    Rigidbody rBody;
    public Transform Target;
    public Transform Wall;
    private float movStrenght = 15.0f;
    private float jumpStrenght =60.0f;
    private bool _isOnGround;
    private float distAnterior;
    RayPerceptionSensorComponent3D rayPerception;
    float timeLeft=30f;


    // Start is called before the first frame update
    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>();
        rBody = GetComponent<Rigidbody>();

        distAnterior = Vector3.Distance(this.transform.localPosition, Target.localPosition);
    }


    public override void CollectObservations(VectorSensor sensor)
    {     
        sensor.AddObservation(Wall.localPosition); //Coleta pos da parede
        sensor.AddObservation(Target.localPosition); //Coleta pos do target
        sensor.AddObservation(this.transform.localPosition); //Coleta pos do agent
        sensor.AddObservation(rBody.velocity.x); //Coleta velocidade x do agent
        sensor.AddObservation(rBody.velocity.z); //Coleta velocidade z do agent
        sensor.AddObservation(rBody.velocity.y); //Coleta velocidade y do agent
        sensor.AddObservation(rayPerception);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 movements = Vector3.zero;
        Vector3 jump = Vector3.zero;
        movements.x = actionBuffers.ContinuousActions[0]; //-1.0f ate 1.0f
        movements.z = actionBuffers.ContinuousActions[1];
        jump.y = actionBuffers.ContinuousActions[2];
    
        rBody.AddForce(movements * movStrenght);
 
        if (jump.y > 0)
        {
            if (_isOnGround==true)
            {
                rBody.AddForce(jump * jumpStrenght*1.0f);
                _isOnGround = false;
            }
        }
        
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        if (distanceToTarget < 1.0f)
        {
            AddReward(1.0f);
            EndEpisode();
        }
        else if (this.transform.localPosition.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetAxis("Jump");

    }

    public override void OnEpisodeBegin()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 1.0f, -6.38f);

        //Se a bolinha estiver caído da plataforma no inicio do episódio, reposicionar bolinha, tirar velocidade.
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Wall"))
        {
            AddReward(-0.5f);
        }
        if (other.collider.CompareTag("Target"))
        {
            timeLeft = Time.time;
            AddReward(1.0f);
            EndEpisode();
        }

    }

    private void FixedUpdate()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            Debug.Log("acabou");
            AddReward(-1.0f);
            timeLeft = Time.time;
            EndEpisode();
        }
        distAnterior = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        
        Debug.DrawRay(transform.position, new Vector3(0f, -1f, 0f) * 0.60f, Color.blue);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, new Vector3(0f, -1f, 0f),out hit, 0.60f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                _isOnGround = true;
            }
            else
            {
                _isOnGround = false;
            }
        }
    }


}



