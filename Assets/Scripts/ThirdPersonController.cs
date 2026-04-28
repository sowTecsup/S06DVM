
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System.Collections;

public class ThirdPersonController : MonoBehaviour
{

    [FoldoutGroup("References")]
    public InputSystem_Actions inputs;
    [FoldoutGroup("References")]
    private CharacterController controller;
    [FoldoutGroup("References")]
    public CinemachineCamera characterCamera;
    [FoldoutGroup("References")]
    public Animator animator;
    [FoldoutGroup("References")]
    public GameObject model;


    [FoldoutGroup("Controller")]
    public float moveSpeed = 5f;

    [FoldoutGroup("Controller")]
    public float runSpeed;

    [FoldoutGroup("Controller")]
    public float rotationSpeed = 200f;

    [FoldoutGroup("Controller/Jump")]
    public float verticalVelocity = 0;

    [FoldoutGroup("Controller/Jump")]
    public float jumpForce = 10;

    [FoldoutGroup("Controller/Jump")]
    public float pushForce = 4;

    [FoldoutGroup("Controller/Jump")]
    public float Gravity = 9.81f;



    [FoldoutGroup("Controller/Dash")]
    private bool IsDashing;

    private bool CanDash = true;
    [FoldoutGroup("Controller/Dash")]
    public float dashForce;

    [FoldoutGroup("Controller/Dash")]
    public float cooldownDash = 5f;
    [FoldoutGroup("Controller/Dash")]
    public float CurrentCDDash;

    [FoldoutGroup("Controller/Dash")]
    public float dashDuration = 0.2f;

    [FoldoutGroup("Controller/Dash")]
    private float dashTimer;

    [SerializeField] private Vector2 moveInput;

    [FoldoutGroup("WallRun")]
    public bool enableWallRun;

    [FoldoutGroup("WallRun")]
    public float MaxStaminaForWallRun = 10f;

    [FoldoutGroup("WallRun")]
    public float CurrentStaminaForWallRun;

    [FoldoutGroup("WallRun")]
    public bool CanWallRun = true;

    [FoldoutGroup("WallRun")]
    public float cameraTitlt = 15;

    [FoldoutGroup("WallRun")]
    public float rayLenght;

    [FoldoutGroup("WallRun")]
    public float airTimeToWallRun = 0.2f;

    private float airTimer;
    Vector3 normalDebug;
    Vector3 impactPoint;
    Vector3 crossResult;
    private void Awake()
    {
        inputs = new();
        controller = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnEnable()
    {
        inputs.Enable();

        inputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;


        inputs.Player.Jump.performed += OnJump;

        inputs.Player.Sprint.performed += ctx =>moveSpeed= moveSpeed + runSpeed;
        inputs.Player.Sprint.canceled += ctx => moveSpeed = moveSpeed - runSpeed;
        inputs.Player.Dash.performed += OnDash;



    }
    void Start()
    {
        CurrentStaminaForWallRun = MaxStaminaForWallRun;
    }
    void Update()
    {
        if (!controller.isGrounded)
        {
            airTimer += Time.deltaTime;
            
        }
        else
        {  
            airTimer = 0;
        }
        OnMove();
        //OnSimpleMove();
        EnableWallRun();

        if(enableWallRun && !controller.isGrounded)
        {
            CurrentStaminaForWallRun -= Time.deltaTime;
            if(CurrentStaminaForWallRun < 0)
            {
                CurrentStaminaForWallRun=0;
                CanWallRun = false;

            }
        }

    }

    public void OnMove()
    {
        Vector3 cameraForwardDir = characterCamera.transform.forward;




        cameraForwardDir.y = 0;
        cameraForwardDir.Normalize();


        if(moveInput != Vector2.zero)
        {
            Quaternion targetQuaternion = Quaternion.LookRotation(cameraForwardDir);
            //transform.rotation = targetQuaternion;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetQuaternion,
                rotationSpeed * Time.deltaTime);


        }

        Vector3 moveDir; ;



        if(!enableWallRun)
        {
            moveDir = (cameraForwardDir * moveInput.y + transform.right * moveInput.x) * moveSpeed;
        }
        else
        {
            moveDir = (cameraForwardDir * moveInput.y) * moveSpeed;
            
        }



        float magnitud = Mathf.Abs(controller.velocity.magnitude);
        // print(magnitud);
        animator.SetFloat("Speed", magnitud);


        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        
        if (enableWallRun && CanWallRun)
            verticalVelocity = 0;
        if(!CanWallRun||controller.isGrounded)
        {

            model.transform.rotation = Quaternion.Euler(0, 0, 0);
            characterCamera.Lens.Dutch = 0;
        }


        //verticalVelocity -= Gravity * Time.deltaTime;
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;


        moveDir.y = verticalVelocity;

        animator.SetBool("Grounded", controller.isGrounded);


        if (IsDashing)
        {
            //->convertir el dash a un barrido por el piso! dash con gravedad integrada omaegoto!
            moveDir = transform.forward * dashForce * (dashTimer / dashDuration);
           
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                IsDashing = false;
        }
        controller.Move(moveDir * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!controller.isGrounded) return;

        animator.SetTrigger("Jump");

        verticalVelocity = jumpForce;
    }
    public void OnSimpleMove()
    {
        transform.Rotate(Vector3.up * moveInput.x * rotationSpeed * Time.deltaTime);
        Vector3 moveDir = transform.forward * moveSpeed * moveInput.y;
        controller.SimpleMove(moveDir);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {


        Vector3 pushDir = (hit.transform.position - transform.position).normalized;

        if (hit.rigidbody != null && hit.rigidbody.linearVelocity == Vector3.zero)
        {
            print(hit.gameObject.name);
            hit.rigidbody.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }
    }
    private void OnDash(InputAction.CallbackContext context)
    {

        if(CanDash)
        {

            IsDashing = true;
            CanDash = false;
            dashTimer = dashDuration;

            StartCoroutine(CooldownDash());
        }
        
    }
    public IEnumerator CooldownDash()
    {
        CurrentCDDash = 0;

        while(CurrentCDDash < cooldownDash)
        {
            CurrentCDDash += Time.deltaTime;
            yield return null;

        }

        CanDash = true;
        yield break;
            
    }
  public void EnableWallRun()
    {
        //->mejor castearlo desde una referenia en los piez
        RaycastHit hit = default;

        Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, rayLenght);

        Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, rayLenght);

        if (hitRight.collider != null && hitRight.collider.gameObject.tag == "Wall")
        {
            hit = hitRight;
            if (enableWallRun)
            {
                characterCamera.Lens.Dutch = cameraTitlt;

                //model.transform.rotation = Quaternion.Euler(-90f, 0, -90);
                animator.SetTrigger("WallRun");

            }

        }
        else if(hitLeft.collider != null && hitLeft.collider.gameObject.tag == "Wall")
        {
            hit = hitLeft;

            if (enableWallRun)
            {
                characterCamera.Lens.Dutch = -cameraTitlt;
                //model.transform.rotation = Quaternion.Euler(90f, 0, 90);
                animator.SetTrigger("WallRun");
            }
               

        }
        else
        {
            characterCamera.Lens.Dutch = 0;
            enableWallRun = false;
        }

        if((hit.collider != null && airTimer >= airTimeToWallRun && CanWallRun))
        {
            enableWallRun = true;
            Debug.Log("AleluyaR");


            normalDebug = hit.normal;
            impactPoint = hit.point;
            crossResult = Vector3.Cross(normalDebug, transform.up);//+1

            if (Vector3.Dot(crossResult, transform.forward) < 0)
            {
                crossResult *= -1;
            }
        }






        /*
        if (hitRight.collider != null &&  hitRight.collider.gameObject.tag == "Wall")
        {



            enableWallRun = true;
            Debug.Log("AleluyaR");

            normalDebug = hitRight.normal;
            impactPoint = hitRight.point;
            crossResult = Vector3.Cross(normalDebug, transform.up);//+1

            if( Vector3.Dot(crossResult,transform.forward) < 0)
            {
                crossResult *= -1;
            }


        }
        else
        {
            enableWallRun =false;
        }

        if (hitLeft.collider != null && hitLeft.collider.gameObject.tag == "Wall")
        {
            Debug.Log("AleluyaL");
        }*/
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.right * rayLenght);
        Gizmos.color = Color.navyBlue;
        Gizmos.DrawRay(transform.position, -transform.right * rayLenght);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(impactPoint, normalDebug * rayLenght);
        Gizmos.DrawSphere(impactPoint, 0.1f);



        Gizmos.color = Color.orange;
        Gizmos.DrawRay(impactPoint,crossResult * rayLenght);


        Gizmos.color = Color.black;
        Gizmos.DrawRay(model.transform.position, model.transform.forward * rayLenght*2);

    }/*
    public void FrontWallAlign()
    {
        if (Physics.Raycast(model.transform.position, model.transform.forward, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.CompareTag("Wall"))
            {

                model.transform.rotation = Quaternion.Euler(-90f, 0,0);
               
            }
        }
    }*/

}
