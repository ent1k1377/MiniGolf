using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    [SerializeField] private float maxPower;
    [SerializeField] private float changeAngleSpeed;
    [SerializeField] private float lineLength;
    [SerializeField] private Slider powerSlider;
    
    [SerializeField] private GameObject Area;
    [SerializeField] private LayerMask rayLayer;
    [SerializeField] private LayerMask areaLM;

    private Camera cam;
    private Rigidbody rBody;
    private LineRenderer line;
    private Vector3 lastPosition;
    private Vector3 inPosition;
    private Vector3 outPosition;
    private bool onKeyDownArea = false;
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private int distanceToTarget = 10;
    private int num_putt = 0;
    private int num_levels = 1;
    private PhotonView photonView;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine) 
        {
            rBody = GetComponent<Rigidbody>();
            rBody.maxAngularVelocity = 1000;
            line = GetComponent<LineRenderer>();
        }
        else
            Area.SetActive(false);
    }

    public void SetupCamera(Camera cam)
    {
        this.cam = cam;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (!cam) return; 

        if (rBody.velocity.magnitude < 0.01f)
        {
            Area.SetActive(true);
            CheckClickArea();
            UpdateLinePosition();

        }
        else
        {
            line.enabled = false;
            Area.SetActive(false);
        }
        Follow();
        FollowBall(Area);
        
    }
    private void FollowBall(GameObject GO)
    {
        GO.transform.position = transform.position;
    }
    private void UpdateLinePosition()
    {
        if (!onKeyDownArea) return;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, transform.InverseTransformPoint(ClickedPoint(rayLayer)));
        line.enabled = true;
    }
    public void CheckClickArea()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && ClickedPoint(areaLM) != Vector3.zero)
        {
            onKeyDownArea = true;
            inPosition = ClickedPoint(rayLayer);

        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && onKeyDownArea)
        {
            outPosition = ClickedPoint(rayLayer);
            Vector3 distance = (inPosition - outPosition) * 10;
            onKeyDownArea = false;
            Putt(distance);

        }
    }


    private void Follow()
    {
        float rotationAroundYAxis = 0;
        
        if (onKeyDownArea) return;
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            currentPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - currentPosition;
            previousPosition = currentPosition;
            rotationAroundYAxis = -direction.x * 180f;
        }

        cam.transform.position = transform.position;
        cam.transform.Rotate(Vector3.up, rotationAroundYAxis, Space.World);
        cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));
    }
    
    Vector3 ClickedPoint(LayerMask LM)
    {
        Vector3 position = Vector3.zero;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LM))
        {
            position = hit.point;
        }
        return position;
    }

   

    private void Putt(Vector3 distance)
    {
        num_putt += 1;
        lastPosition = transform.position;
        rBody.AddForce(distance * maxPower, ForceMode.Impulse);
    }

    private bool isWin = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.tag == "Hole")
        {
            if (num_levels == 3)
            {
                if (!isWin)
                {
                    GameManager.instance.photonView.RPC("UpdatePlace", RpcTarget.All, PhotonNetwork.NickName);
                    StartCoroutine(GameManager.instance.UpdateCoins(PhotonNetwork.NickName));
                    isWin = true;
                }
                
            }
            else
            {
                if (rBody)
                {
                    rBody.velocity = Vector3.zero;
                    rBody.angularVelocity = Vector3.zero;
                    float[,] levels = GameManager.instance.levels;
                    Vector3 spawn_ball = new Vector3(levels[num_levels, 0], levels[num_levels, 1], levels[num_levels, 2]);
                    transform.position = spawn_ball;
                    num_levels += 1;
                }
                
            }
            
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (collision.collider.tag == "ExitField")
        {
            
            transform.position = lastPosition;
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }

    }
}
