using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 2f;

    private Transform target;
    private Animator anim;
    private bool isWaiting = false;
    private float waitTimer;

    private float reachDistance = 0.1f;

    void Start()
    {
        target = pointB;
        anim = GetComponent<Animator>();

        FaceTarget();
        anim.SetBool("isWalking", true);
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;

                FaceTarget();
                anim.SetBool("isWalking", true);
            }
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) <= reachDistance)
        {
            anim.SetBool("isWalking", false);

            isWaiting = true;
            waitTimer = waitTime;

            target = (target == pointA) ? pointB : pointA;
        }
    }

    void FaceTarget()
    {
        Vector3 scale = transform.localScale;

        if (target.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x); 
        else
            scale.x = Mathf.Abs(scale.x); 

        transform.localScale = scale;
    }
}
