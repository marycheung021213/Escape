using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using myGabber;

public class materialController : MonoBehaviour
{
    [Header("Layers")]
	[SerializeField]
    LayerMask collisionLayer;

	float m_grabMinDistance;
	float m_grabMaxDistance;

    Grabber grabber;
    bool m_isgrabbing = false;

    Transform m_transform;

    private bool lastFrameInfo;

    public Renderer renderer;
    public Material NormalMat;
    public Material StencilMat;
    // Start is called before the first frame update
    void Start()
    {
        grabber = GetComponent<Grabber>();
        m_grabMaxDistance = grabber.m_grabMaxDistance;
        m_grabMinDistance = grabber.m_grabMinDistance;
        m_transform = grabber.m_transform;
        lastFrameInfo = false;
        m_isgrabbing = grabber.m_grabbing;
        renderer.gameObject.tag = "InPainting";
    }

    // Update is called once per frame
    void Update()
    {
        m_isgrabbing = grabber.m_grabbing;
        m_transform = grabber.m_transform;
        if(m_isgrabbing){
            RaycastHit hit;
            Ray ray = new Ray(m_transform.position,m_transform.forward);
            if(Physics.Raycast(ray,out hit,m_grabMaxDistance,collisionLayer) != lastFrameInfo){
                if(!lastFrameInfo){
                    renderer.material = StencilMat;
                    renderer.gameObject.tag = "InPainting";

                    
                }else{
                    renderer.material = NormalMat;
                    renderer.gameObject.tag = "OutofPainting";
                }
                
            }

            lastFrameInfo = Physics.Raycast(ray,out hit,m_grabMaxDistance,collisionLayer);

        }else{
            if(renderer.gameObject.tag == "InPainting"){
                renderer.gameObject.GetComponent<Rigidbody>().useGravity = false;
            }else{
                renderer.gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
}
