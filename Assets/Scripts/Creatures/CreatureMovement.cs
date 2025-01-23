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
    private float minRestTime = 2f;
    private float maxRestTime = 8f;
    private float restTimer;
    private bool isResting;
    private float groundCheckDistance = 0.1f;
    private float maxSlopeAngle = 45f;
    private float heightOffset = 0.5f; // Offset from ground to avoid clipping
    private Vector3 groundNormal;
    private float energyCostMultiplier = 0.1f;
    private float slopePenalty = 0.5f;

    public void Initialize(Creature creature)
    {
        this.creature = creature;
        moveSpeed = Mathf.Lerp(5f, 2f, creature.dna.size);
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

        MoveCreature();
        CheckTargetReached();
        ConsumeEnergy();
    }

    private void HandleResting()
    {
        restTimer -= Time.deltaTime;
        if (restTimer <= 0)
        {
            isResting = false;
            SetNewRandomTarget();
        }
    }

    private void MoveCreature()
    {
        // Calculate direction to target
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Check if the slope is too steep
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, groundCheckDistance + 1f))
        {
            groundNormal = hit.normal;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            if (slopeAngle > maxSlopeAngle)
            {
                // Find a new target if current path is too steep
                SetNewRandomTarget();
                return;
            }

            // Adjust movement speed based on slope
            float currentSpeed = moveSpeed * (1f - (slopeAngle / maxSlopeAngle) * slopePenalty);

            // Move the creature
            Vector3 movement = directionToTarget * currentSpeed * Time.deltaTime;
            transform.position += movement;

            // Adjust height based on terrain
            float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
            transform.position = new Vector3(
                transform.position.x,
                terrainHeight + heightOffset,
                transform.position.z
            );

            // Rotate to face movement direction while aligning with terrain
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, groundNormal);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private void CheckTargetReached()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        Debug.Log($"distance to target = {distanceToTarget}");
        if (distanceToTarget < 10f)
        {
            isResting = true;
            restTimer = Random.Range(minRestTime, maxRestTime);
        }
    }

    private void ConsumeEnergy()
    {
        // Calculate energy cost based on size, speed, and slope
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        float energyCost = moveSpeed * creature.dna.size *
            (1f + (slopeAngle / maxSlopeAngle)) * energyCostMultiplier * Time.deltaTime;

        creature.dna.health -= energyCost;
    }

    private void SetNewRandomTarget()
    {
        // Get random point within wanderRadius
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Ensure target is within terrain bounds and adjust height
        if (Terrain.activeTerrain != null)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, 0, Terrain.activeTerrain.terrainData.size.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, 0, Terrain.activeTerrain.terrainData.size.z);
            targetPosition.y = Terrain.activeTerrain.SampleHeight(targetPosition) + heightOffset;
        }
    }
}
