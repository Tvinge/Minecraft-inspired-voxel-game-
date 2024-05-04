using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpforce = 5f;
    public float gravity = -9.807f;

    public float playerWidth = 0.15f;


    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private  float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highLightBlock;
    public Transform PlaceBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    
    public byte selectedBlockIndex = 1;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
        //selectedBlockText.text = world.blocktypes[selectedBlockIndex].blockName + " block selected";

    }
    private void FixedUpdate()
    {

        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity);//right means
        transform.Translate(velocity, Space.World);

        //world.inUI = false;
    }
    private void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.I))
        {
            world.InUI = !world.InUI;
        }
        if (!world.InUI)
        {
            GetPlayerInputs();
            placeCursorBlocks();
        }*/
        GetPlayerInputs();
        placeCursorBlocks();
    }
    void Jump()
    {
        verticalMomentum = jumpforce;
        isGrounded = false;
        jumpRequest = false;
    }
    private void CalculateVelocity ()
    {
        //affect vertuical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;
        //if we are sprinting, use the sprint multi
        if(isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        //apply vertical momentum falling/jumping
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);

                
    }
    private void GetPlayerInputs()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint")) 
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        
        if (highLightBlock.gameObject.activeSelf)
        {//destroy  block
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(highLightBlock.position).EditVoxel(highLightBlock.position, 0);
            //place block
            if (Input.GetMouseButtonDown(1))
                world.GetChunkFromVector3(PlaceBlock.position).EditVoxel(PlaceBlock.position, selectedBlockIndex);
        }
    }
    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        
        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);
            
            if (world.CheckForVoxel(pos))
            {
                highLightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                PlaceBlock.position = lastPos;

                highLightBlock.gameObject.SetActive(true);
                PlaceBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;

        }
        highLightBlock.gameObject.SetActive(false);
        PlaceBlock.gameObject.SetActive(false);
    }
    private float checkDownSpeed(float downSpeed)//checks downword collistions
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)))
        {
            isGrounded = true;
            return 0;
        } 
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }
    private float checkUpSpeed(float upSpeed)//checks upward collisitions
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
    public bool front//chcecks collistions on fornt of the player, 2 times bcs he is 2 blocks tall
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))
                if (world.settings.AutoJumpOn == true && world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)))
                    return jumpRequest = true;
                else
                    return true;
            else
                return false;

        }
    }
    public bool back//chcecks collistions on back of the player, 2 times bcs he is 2 blocks tall
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
                if(world.settings.AutoJumpOn == true && world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)))
                    return jumpRequest = true;
                else
                    return true;
            else
                return false;
        }
    }
    public bool left//chcecks collistions on left of the player, 2 times bcs he is 2 blocks tall
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))
                if (world.settings.AutoJumpOn == true && world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)))
                    return jumpRequest = true;
                else

                    return true;
            else
                return false;
        }
    }
    public bool right//chcecks collistions on right of the player, 2 times bcs he is 2 blocks tall
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))
                if (world.settings.AutoJumpOn == true && world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)))
                    return jumpRequest = true;
                else
                    return true;
            else
                return false;
        }
    }
}
