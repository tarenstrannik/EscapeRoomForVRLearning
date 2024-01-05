using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class DoorHandle : XRBaseInteractable
{
    [Header("Door options")]
    [SerializeField] private Transform m_door;
    private Vector3 m_doorRelativeToHandle;
    private Vector3 m_doorMovementVector;
    [SerializeField] private float m_forceCoef = 50f;

    private Rigidbody m_rigidbody;
    private Vector3 m_startHandlePosition;
    [SerializeField] private float m_frictionForceCoefficient = 10f;
    private XRBaseController m_currentController;

    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private float m_audioAndHapticsBaseVelocity = 0.2f;
    [SerializeField] private float m_minHaptics = 0.1f;


    [SerializeField] private Transform m_attachTransform;
    private Transform m_controllerAttach;
    private Transform m_handModel;
    protected override void Awake()
    {
        base.Awake();
        m_doorRelativeToHandle = transform.InverseTransformPoint(m_door.position);
        m_rigidbody= GetComponent<Rigidbody>();
        m_doorMovementVector = transform.InverseTransformVector(-transform.right).normalized;

        m_startHandlePosition = transform.position;
    }

    private void FixedUpdate()
    {
        CheckRestrictions();
        PlaySound();
        //ApplyFriction();
        m_door.position = transform.TransformPoint(m_doorRelativeToHandle);

        
    }
    private void ApplyFriction()
    {
        if (m_rigidbody.velocity.magnitude>0)
        {
                m_rigidbody.AddForce(-m_rigidbody.velocity.normalized * m_frictionForceCoefficient, ForceMode.Impulse);
        }

    }
    private void CheckRestrictions()
    {
        var relativeToStart = transform.InverseTransformPoint(m_startHandlePosition).x;
        if(relativeToStart<0)
        {
            m_rigidbody.velocity = Vector3.zero;
            transform.position = m_startHandlePosition;
        }
        transform.position = new Vector3(m_startHandlePosition.x, m_startHandlePosition.y, transform.position.z);
    }
    private void PlaySound()
    {
        var velocity = m_rigidbody.velocity.magnitude;
        m_audioSource.pitch = velocity / m_audioAndHapticsBaseVelocity;
        if(velocity > 0)
        {
            if(!m_audioSource.isPlaying)
            {
                m_audioSource.Play();
            }
        }
        else
        {
            m_audioSource.Pause();
        }
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        var controllerInteractor = args.interactorObject as XRBaseControllerInteractor;
        if (controllerInteractor != null) m_currentController = controllerInteractor.xrController;

        m_handModel = m_currentController.GetComponentInChildren<Renderer>().gameObject.transform.parent.transform;
        m_controllerAttach = m_currentController.transform;
        AttachHandModelToTransform(m_attachTransform);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        AttachHandModelToTransform(m_controllerAttach);
        m_currentController = null;
        m_handModel = null;
        m_controllerAttach = null;

    }
    private void AttachHandModelToTransform(Transform transf)
    {
        m_handModel.position = transf.position;
        m_handModel.rotation = transf.rotation;
        m_handModel.SetParent(transf);
    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if(m_currentController != null && updatePhase== XRInteractionUpdateOrder.UpdatePhase.Fixed)
        {
            ApplyForce();
            PlayHaptics();
        }
    }
    private void PlayHaptics()
    {
        var velocity = m_rigidbody.velocity.magnitude;
        if (velocity > 0)
        {
            var haptics = Mathf.Clamp(m_audioAndHapticsBaseVelocity / velocity, m_minHaptics, 1);
            m_currentController.SendHapticImpulse(haptics, Time.deltaTime);
        }
    }
    private void ApplyForce()
    {
        var relativePos = transform.InverseTransformPoint(m_currentController.transform.position).normalized;
        var dot = Vector3.Dot(m_doorMovementVector, relativePos);
        Vector3 localforce = transform.forward*dot* m_forceCoef;
        Vector3 force = transform.TransformVector(localforce);
        
        
        m_rigidbody.AddForce(force, ForceMode.Impulse);

        
    }
}
