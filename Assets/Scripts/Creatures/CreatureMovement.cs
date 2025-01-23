// using System.Numerics;
using UnityEditor;
using UnityEngine;

public class CreatureMovement : MonoBehaviour
{
    private Creature creature;
    private Vector3 targetPosition;
    private float moveSpeed;
    private float rotationSpeed = 5f;
    private float wanderRadius = 50f;
    private float minRestTime = 1f;
    private float maxRestTime = 5f;
    private float restTimer;
    private bool isResting;
    private float groundCheckDistance = 0.1f;
    private float maxSlopeAngle = 90f;
    private float heightOffset = 0.5f; // Offset from ground to avoid clipping
    private Vector3 groundNormal;
    private float energyCostMultiplier = 0.1f;
    private float slopePenalty = 0.5f;
    private float idleTime = 0f;

    public void Initialize(Creature creature)
    {
        this.creature = creature;
        moveSpeed = Mathf.Lerp(5f, 2f, Mathf.InverseLerp(1f, 5f, creature.dna.size)); // Adjusted for size range
        restTimer = Random.Range(minRestTime, maxRestTime);
        SetNewRandomTarget();
    }

    private void Update()
    {
        if (creature == null) return;

        if (isResting)
        {
            HandleResting();
            return;
        }

        idleTime += Time.deltaTime;

        MoveCreature();
        CheckTargetReached();

        if (idleTime > 5f) // If idle for 5 seconds
        {
            SetNewRandomTarget();
            idleTime = 0f;
        }

        ConsumeEnergy();
    }

    private void HandleResting()
    {
        restTimer -= Time.deltaTime;

        // Recover health while resting
        creature.dna.health += 10f * Time.deltaTime;
        creature.dna.health = Mathf.Clamp(creature.dna.health, 0, 100f);

        if (restTimer <= 0)
        {
            isResting = false;
            SetNewRandomTarget();
        }
    }

    private void MoveCreature()
    {
        // Calculate direction to target and reset idle timer if movement occurs
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float movementMagnitude = (directionToTarget * moveSpeed * Time.deltaTime).magnitude;
        if (movementMagnitude > 0.01f) idleTime = 0f;

        // Check if the slope is too steep
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, groundCheckDistance + heightOffset))
        {
            groundNormal = hit.normal;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            // Adjust movement speed based on slope
            float sizeFactor = Mathf.Lerp(1f, 2f, creature.dna.size / 5f); // Boost for larger creatures
            float currentSpeed = moveSpeed * (1f - (slopeAngle / maxSlopeAngle) * (slopePenalty * 0.5f));
            currentSpeed = Mathf.Max(currentSpeed, 0.5f); // Ensure minimum speed

            if (slopeAngle > maxSlopeAngle)
            {
                SetNewRandomTarget();
                return; // Too steep, don't move
            }

            // Move the creature
            Vector3 movement = directionToTarget * currentSpeed * Time.deltaTime;
            if (movement.magnitude > 0.0001f) transform.position += movement;

            // Adjust height based on terrain
            float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
            transform.position = new Vector3(
                transform.position.x,
                terrainHeight + heightOffset + (creature.dna.size * 0.1f), // Increase offset proportional to size
                transform.position.z
            );

            // Rotate to face movement direction while aligning with terrain
            // only when there is movement
            if (movement.magnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, groundNormal);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }
        }
    }

    private void CheckTargetReached()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget < 1f)
        {
            isResting = true;
            restTimer = Random.Range(minRestTime, maxRestTime);
        }
    }

    private void ConsumeEnergy()
    {
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        // Calculate energy cost based on size, speed, and slope
        float energyCost = moveSpeed * Mathf.Clamp(creature.dna.size, 1f, 5f) * (1f + (slopeAngle / maxSlopeAngle))
            * energyCostMultiplier * Time.deltaTime;

        // Reduce health but ensure it doesn't go below 0
        creature.dna.health -= energyCost;
        if (creature.dna.health < 0f) creature.dna.health = 0f;
    }

    private void SetNewRandomTarget()
    {
        // Get random point within wanderRadius
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Ensure target is within terrain bounds and adjust height
        if (Terrain.activeTerrain != null)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, wanderRadius, Terrain.activeTerrain.terrainData.size.x - wanderRadius);
            targetPosition.z = Mathf.Clamp(targetPosition.z, wanderRadius, Terrain.activeTerrain.terrainData.size.z - wanderRadius);
            targetPosition.y = Terrain.activeTerrain.SampleHeight(targetPosition) + heightOffset;
        }
    }
}
