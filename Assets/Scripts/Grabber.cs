using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace myGabber{

[System.Serializable]
public class GrabObjectProperties{
	
	public bool m_useGravity = false;
	public float m_drag = 10;
	public float m_angularDrag = 10;
	public RigidbodyConstraints m_constraints = RigidbodyConstraints.FreezeRotation;		

}
public class Grabber : MonoBehaviour
{
    [Header("Grab properties")]

	[SerializeField]
	[Range(4,50)]
	float m_grabSpeed = 7;

	[SerializeField]
	[Range(0.1f ,5)]
	public float m_grabMinDistance = 1;

	[SerializeField]
	[Range(4 ,25)]
	public float m_grabMaxDistance = 10;

	[SerializeField]
	[Range(0.1f,1)]
	float m_scrollWheelSpeed = 1;

	[SerializeField]
	[Range(50,500)]
	float m_angularSpeed = 300;

	[SerializeField]
	[Range(10,50)]
	float m_impulseMagnitude = 25;



    [Header("Affected Rigidbody Properties")]
	[SerializeField] GrabObjectProperties m_grabProperties = new GrabObjectProperties();	

	GrabObjectProperties m_defaultProperties = new GrabObjectProperties();

    [Header("Layers")]
	[SerializeField]
	LayerMask m_collisionMask;

    Rigidbody m_targetRB = null;

	public Transform m_transform;	

	public Transform[] m_transforms;

	Vector3 m_targetPos;
	GameObject m_hitPointObject;
	float m_targetDistance;

	public bool m_grabbing = false;
	bool m_applyImpulse = false;
	bool m_isHingeJoint = false;

	//Debug
	LineRenderer m_lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
		m_hitPointObject = new GameObject("Point");

		m_lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
		
		bool leftGrab;
		bool rightGrab;
		InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out leftGrab);
		InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out rightGrab);
        if( m_grabbing )
		{

			
			bool bButton;
			InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bButton);
			m_targetDistance += bButton? m_scrollWheelSpeed:0;

			bool aButton;
			InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out aButton);
			m_targetDistance -= aButton? m_scrollWheelSpeed:0;

			m_targetDistance = Mathf.Clamp(m_targetDistance , m_grabMinDistance , m_grabMaxDistance);

			m_targetPos = m_transform.position + m_transform.forward * m_targetDistance;
						
			if(!m_isHingeJoint){
                m_targetRB.constraints = m_grabProperties.m_constraints;
			}
			

			if( Input.GetMouseButtonUp(0) || (!leftGrab && m_transform == m_transforms[0]) || (!rightGrab && m_transform == m_transforms[1])){				
				Reset();
				m_grabbing = false;
			}else if ( Input.GetMouseButtonDown(1) ){
				m_applyImpulse = true;
			}

			
		}
        else
        {
			if(leftGrab && m_transform == null){
				m_transform = m_transforms[0];
			}
			if(rightGrab && m_transform == null){
				m_transform = m_transforms[1];
			}
			if(m_transform != null)
			{
				RaycastHit hitInfo;
				if(Physics.Raycast(m_transform.position , m_transform.forward , out hitInfo , m_grabMaxDistance , m_collisionMask ))
				{
					Rigidbody rb = hitInfo.collider.GetComponent<Rigidbody>();
					if(rb != null){							
						Set( rb , hitInfo.distance);						
						m_grabbing = true;
					}
				}
			}
        }


    }

	void Set(Rigidbody target , float distance)
	{	
		m_targetRB = target;
		m_isHingeJoint = target.GetComponent<HingeJoint>() != null;		

		//Rigidbody default properties	
		m_defaultProperties.m_useGravity = m_targetRB.useGravity;	
		m_defaultProperties.m_drag = m_targetRB.drag;
		m_defaultProperties.m_angularDrag = m_targetRB.angularDrag;
		m_defaultProperties.m_constraints = m_targetRB.constraints;

		//Grab Properties	
		m_targetRB.useGravity = m_grabProperties.m_useGravity;
		m_targetRB.drag = m_grabProperties.m_drag;
		m_targetRB.angularDrag = m_grabProperties.m_angularDrag;
		m_targetRB.constraints = m_isHingeJoint? RigidbodyConstraints.None : m_grabProperties.m_constraints;
		
		
		m_hitPointObject.transform.SetParent(target.transform);							

		m_targetDistance = distance;
		m_targetPos = m_transform.position + m_transform.forward * m_targetDistance;

		m_hitPointObject.transform.position = m_targetPos;
		m_hitPointObject.transform.LookAt(m_transform);
				
	}

    void Reset()
	{		
		//Grab Properties	
		m_targetRB.useGravity = m_defaultProperties.m_useGravity;
		m_targetRB.drag = m_defaultProperties.m_drag;
		m_targetRB.angularDrag = m_defaultProperties.m_angularDrag;
		m_targetRB.constraints = m_defaultProperties.m_constraints;
		
		m_targetRB = null;

		m_hitPointObject.transform.SetParent(null);
		
		if(m_lineRenderer != null)
			m_lineRenderer.enabled = false;

		m_transform = null;
	}

    void FixedUpdate()
	{
		if(!m_grabbing)
			return;
		
		Grab();		

		if(m_applyImpulse){
			m_targetRB.velocity = m_transform.forward * m_impulseMagnitude;
			Reset();
			m_grabbing = false;
			m_applyImpulse = false;
		}
		
	}

    void Grab()
	{
		Vector3 hitPointPos = m_hitPointObject.transform.position;
		Vector3 dif = m_targetPos - hitPointPos;

		if(m_isHingeJoint)
			m_targetRB.AddForceAtPosition( m_grabSpeed  * dif * 100 , hitPointPos , ForceMode.Force);
		else
			m_targetRB.velocity = m_grabSpeed * dif;		

		
		if(m_lineRenderer != null){
			m_lineRenderer.enabled = true;
			m_lineRenderer.SetPositions( new Vector3[]{ m_targetPos , hitPointPos });
		}
	}


}

}
