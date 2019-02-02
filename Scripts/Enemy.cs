using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State {Idle, Chasing, Attacking};
    State currentState;

    public ParticleSystem deathEffect;

    private NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;
    Color originalColour;

    public float attackDistanceThreshold = 0.5f;
    public float timeBetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadious;

    bool hasTarget;

    private void Awake() {
        pathfinder = GetComponent<NavMeshAgent> ();

        //Only if Player exist.
        if (GameObject.FindGameObjectWithTag ("Player") != null) {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag ("Player").transform;

            //Subscribe to On death event of the Player
            targetEntity = target.GetComponent<LivingEntity> ();

            myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            targetCollisionRadious = target.GetComponent<CapsuleCollider> ().radius;
        }
    }

    protected override void Start () {
        base.Start ();

        //Only if Player exist.
        if(hasTarget) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine (UpdatePath ());
        }
	}

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour) {
        pathfinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer> ().sharedMaterial;
        skinMaterial.color = skinColour;
        originalColour = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        if(damage >= health) {
            Destroy(Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetimeMultiplier);
        }

        base.TakeHit (damage, hitPoint, hitDirection);
    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }
	
	void Update () {
        //Has a player to attack
        if (hasTarget) {
            //Ready to attack
            if (Time.time > nextAttackTime) {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

                //Close enough to attack
                if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadious, 2)) {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine (Attack ());
                }
            }
        }
	}

    IEnumerator Attack() {

        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;

        //Direction to target
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while( percent <= 1) {

            if(percent >= 0.5f && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage (damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColour;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath() {
        float refreshRate = 0.25f;

        while(hasTarget) {
            // only update the path when in Chasing state
            if(currentState == State.Chasing) {
                //Direction to target
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadious + attackDistanceThreshold/2);
                if (!dead) {
                    pathfinder.SetDestination (targetPosition);
                }
            }
            yield return new WaitForSeconds (refreshRate);
        }
    }
}
