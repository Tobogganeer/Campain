using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ViewCone))]
[RequireComponent(typeof(HitPoints))]
[DisallowMultipleComponent]
public class RangedAI : MonoBehaviour, IDamagable
{
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public ViewCone vision;
    [HideInInspector] public HitPoints health;
    [HideInInspector] public Ragdoller ragdoller;
    [HideInInspector] public new Collider collider; // Basic references

    public float rotationSpeed = 3; // How fast the agent rotates
    public float poiRandomizationRadius = 2; // When choosing a point of interest, what randomness should be added
    public float stoppedMovingTimeThreshold = 2; // How much time spent not moving until the ai does something else
    public float destinationMinDistance = 2; // How close the agent must get to a destination to do something else
    public float tryKeepDistanceFromPlayer = 5; // Try to stay this far away from the player
    public float playerSurroundRandomizeTime = 1f; // The ai will move in a circle around the player. Randomize the point after this time
    public float surroundRandomizeRadius = 3; // What random radius to run around the player in

    public float attackCooldownMin = 0.1f; // Attacks use a random time between these min and max values
    public float attackCooldownMax = 2f;
    //public GameObject attackPrefab; // Obj to spawn on attack
    public Transform attackFrom; // Where to spawn obj
    public float playerPredictionSeconds = 0.3f; // How far into the future to predict the player to be
    public float predictionDeltaSpeed = 1; // How fast the prediction converges
    public float randomTargetRadius = 1; // Innaccuracy, pretty much
    public float maxAttackRange = 15; // Don't attack unless the player is within this range

    public float sensoryRange = 5; // If the player is closer than this, detect the player even if not in view

    private Vector3 lastPredictionDelta;
    private Vector3 predictionDelta;

    private float currentAttackTime = 0;

    private float currentSurroundTime = 0;

    private Vector3? lastSeenPlayerPos;
    private Vector3? currentPointOfInterest;
    private float lastSeenPlayerTime;

    private Vector3? lookTarget;

    private Vector3 surroundOffset;

    private const int REPEAT_FRAME_COUNT = 5; // Run the evaluation after this many frames

    private float stoppedTime;

    private bool moving => IsMoving();

    //private LineRenderer debugLineRenderer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<ViewCone>();
        health = GetComponent<HitPoints>();
        ragdoller = GetComponent<Ragdoller>();
        collider = GetComponent<Collider>();

        agent.updateRotation = false;
        // Get components, stop rotation as we rotate the agent manually
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.U)) ragdoller.EnableRagdoll();

        if (!enabled || health.CurrentHP <= 0) return;
        // Dont think if this is dead

        if (Time.frameCount % REPEAT_FRAME_COUNT == 0)
            EvaluateAI(); // Update every REPEAT_FRAME_COUNT'th frame

        if (agent.velocity.sqrMagnitude > 0.1f)
            stoppedTime = 0;
        else
            stoppedTime += Time.deltaTime;
        // Increase stoppedTime if agent is slow

        if (currentSurroundTime > 0)
            currentSurroundTime -= Time.deltaTime;
        // Decrease timers

        currentAttackTime -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!enabled || health.CurrentHP <= 0) return;

        UpdateRotation();
    }

    #region Decision Making
    private void UpdateRotation()
    {
        // Smoothly look towards target if it exists
        if (lookTarget != null)
        {
            Quaternion currentRotation = transform.rotation;
            transform.LookAt(lookTarget.Value);
            float y = transform.eulerAngles.y;

            // float angle = Mathf.Atan2(dirTowardsPlayer.x, dirTowardsPlayer.z);

            Quaternion desiredRot = Quaternion.Euler(new Vector3(0, y, 0));
            transform.rotation = Quaternion.Slerp(currentRotation, desiredRot, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Otherwise look towards current destination
            Quaternion currentRotation = transform.rotation;
            //transform.LookAt(Player.Position);
            if (agent.velocity.sqrMagnitude > 0.1f)
                transform.LookAt(transform.position + agent.velocity);
            float y = transform.eulerAngles.y;

            // float angle = Mathf.Atan2(dirTowardsPlayer.x, dirTowardsPlayer.z);

            Quaternion desiredRot = Quaternion.Euler(new Vector3(0, y, 0));
            transform.rotation = Quaternion.Slerp(currentRotation, desiredRot, Time.deltaTime * rotationSpeed);
        }
    }

    private void EvaluateAI()
    {
        bool canSeePlayer = vision.CanSee(PlayerMovement.Position);

        // If agent can see the player or the player is really close
        if (canSeePlayer || Vector3.Distance(transform.position, PlayerMovement.Position) < sensoryRange)
            OnPlayerInSight();

        // Otherwise if the ai has seen the player
        else if (lastSeenPlayerPos != null)
            OnPlayerWasSeen();

        // Otherwise just do this
        else
            OnPlayerLocationUnknown();
    }

    private void OnPlayerInSight()
    {
        Vector3 playerPos = PlayerMovement.Position;

        lastSeenPlayerPos = playerPos;
        lastSeenPlayerTime = Time.time;
        currentPointOfInterest = null;
        lookTarget = playerPos;
        // Set values

        if (agent.destination != playerPos)
        {
            // If not already going the players position
            Vector3 offset = Vector3.zero;

            if (currentSurroundTime <= 0)
            {
                // If needing a new surround randomization
                currentSurroundTime = playerSurroundRandomizeTime;
                surroundOffset = Random.insideUnitCircle.XYToXZ().normalized * surroundRandomizeRadius;
            }

            if (transform.position.SqrDistance(playerPos) < tryKeepDistanceFromPlayer * tryKeepDistanceFromPlayer)
            {
                Vector3 awayDirection = playerPos.DirectionTo(transform.position);
                offset += awayDirection * tryKeepDistanceFromPlayer;
            }

            agent.SetDestination(playerPos + offset + surroundOffset);
        }

        //TryAttack(playerPos);
        TryAttack();
    }

    private void OnPlayerWasSeen()
    {
        currentPointOfInterest = null;

        if (lastSeenPlayerPos != null && agent.destination != lastSeenPlayerPos && !moving)
        {
            lookTarget = lastSeenPlayerPos.Value;
            agent.SetDestination(lastSeenPlayerPos.Value);
        }

        if ((agent.destination == lastSeenPlayerPos &&
            transform.position.SqrDistance(agent.destination) < destinationMinDistance * destinationMinDistance) || !moving)
        {
            lastSeenPlayerPos = null;
        }
    }

    private void OnPlayerLocationUnknown()
    {
        lastSeenPlayerPos = null;
        lookTarget = null;

        if (moving) return;

        currentPointOfInterest = AIPointManager.GetPointOfInterest(transform.position);
        Vector3 offset = Random.insideUnitCircle.XYToXZ() * poiRandomizationRadius;
        agent.SetDestination(currentPointOfInterest.Value + offset);
    }

    #endregion

    private void TryAttack()//Vector3 target)
    {
        if (currentAttackTime > 0 || attackFrom.position.SqrDistance(PlayerMovement.Position) > maxAttackRange * maxAttackRange) return;

        currentAttackTime = Random.Range(attackCooldownMin, attackCooldownMax);

        //predictionDelta = Player.Controller.RB.velocity * playerPredictionSeconds;
        //predictionDelta = Vector3.Lerp(lastPredictionDelta, predictionDelta, Time.deltaTime * predictionDeltaSpeed);
        //Vector3 predictedPos = PlayerMovement.Position + (Random.insideUnitSphere * randomTargetRadius) + predictionDelta;

        //Vector3 dir = attackFrom.position.DirectionTo(predictedPos);
        //Debug.DrawRay(attackFrom.position, dir * 50, Color.blue, 0.4f);
        Bullet.Create(attackFrom.position, attackFrom.position.DirectionTo(PlayerMovement.Position + Random.insideUnitSphere * 3), WeaponManager.CurrentWeapon);
        AudioManager.Play(new Audio("NP5_Fire_Default").SetPosition(attackFrom.position).SetParent(transform).SetDistance(75));
        //AudioManager.Play(AudioArray.NP5_Fire_Default, attackFrom.position, transform, 75);

        //GameObject proj = Instantiate(attackPrefab, attackFrom.position, Quaternion.LookRotation(dir, Vector3.up));
        //proj.GetComponent<EnemyProjectile>().Init(predictedPos, attackFrom.position);
        //Physics.IgnoreCollision(proj.GetComponent<Collider>(), collider, true);
        //lastPredictionDelta = predictionDelta;
    }

    private bool IsMoving()
    {
        return stoppedTime < stoppedMovingTimeThreshold;
    }

    private void OnDrawGizmosSelected()
    {
        if (lastSeenPlayerPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, lastSeenPlayerPos.Value);
        }

        if (currentPointOfInterest != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentPointOfInterest.Value);
        }

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(agent.destination, 0.4f);

        Gizmos.color = new Color(1, 0, 1);
        Gizmos.DrawLine(attackFrom.position, attackFrom.position + lastPredictionDelta * 4);
    }

    public void TakeDamage(DamageDetails details)
    {
        if (health.CurrentHP <= 0) return;

        AudioManager.Play(new Audio("UIClick").Set2D());

        health.TakeDamage(details.amount);

        if (health.CurrentHP <= 0)
        {
            Die();
            AddForceToRagdoll(details.direction * 10, ForceMode.Impulse);
        }
    }

    private void Die()
    {
        AudioManager.Play(new Audio("EnemyDie").SetPosition(transform.position).SetVolume(0.5f));
        //AudioManager.Play(AudioArray.EnemyDie, transform.position, null, 10, AudioCategory.SFX, 0.5f);
        ragdoller.EnableRagdoll();
        EnemyEvents.EnemyDied(transform.position);
    }

    public void AddForceToRagdoll(Vector3 force, ForceMode forceMode)
    {
        ragdoller.AddForce(force, forceMode);
    }


    private void OnEnable()
    {
        EnemyEvents.OnEnemyDie += EnemyEvents_OnEnemyDie;
    }

    private void OnDisable()
    {
        EnemyEvents.OnEnemyDie -= EnemyEvents_OnEnemyDie;
    }

    private void EnemyEvents_OnEnemyDie(Vector3 position)
    {
        if (health.CurrentHP <= 0) return;

        if (Vector3.Distance(transform.position, position) < sensoryRange)
        {
            lastSeenPlayerPos = position;
            currentPointOfInterest = null;
            lookTarget = position;
            agent.SetDestination(position);
        }
    }
}

/*

AI Behaviour Outline:

Repeat

CanSeePlayer
    Health > a_value
        ChasePlayer
        KeepDistance
        Attack
    Health < a_value
        FindCover
        Attack
HasSeenPlayer
    Health > a_value
        GoToLastSeenPosition
    Health < a_value
        GoOppositeDirection
        FindHealth/Heal
GoToPointOfInterest

*/

