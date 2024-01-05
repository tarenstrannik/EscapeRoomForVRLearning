using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;


public class CardReader : XRSocketInteractor
{
    [Header("CardReader options")]
    [SerializeField] private Renderer m_successLamp;
    [SerializeField] private Renderer m_errorLamp;
    [SerializeField] private float m_blinkTime;

    [SerializeField] private float m_minSpeed = 1f;
    [SerializeField] private float m_maxSpeed = 2f;
    [SerializeField] private Transform m_attachedPosition;
    private float m_startMovingTime = 0f;
    private float m_movingDistance = 0f;

    private Vector3 m_updatedrelativePosition= Vector3.zero;
    private Vector3 m_prevLocalCardPosition= Vector3.zero;
    private Vector3 m_readingCardLocalForward;
    private float m_attachedRelativeX;
    private float m_attachedRelativeY;
    [SerializeField] private float m_minMoveDistance = 0.05f;
    [SerializeField] private float m_minDistanceToSocket=0.1f;
    public UnityEvent m_success;
    public UnityEvent m_error;


    private Coroutine m_blinking;

    private GameObject m_card;
    private XRGrabInteractable m_cardGrabInteractable;
    private bool m_cardInCardReader = false;
    private XRBaseController m_currentController;


    protected override void Awake()
    {
        base.Awake();
        m_success.AddListener(Success);
        m_error.AddListener(Error);

        var attachedPosition = transform.InverseTransformPoint(m_attachedPosition.position);
        m_attachedRelativeX = attachedPosition.x;
        m_attachedRelativeY= attachedPosition.y;
        m_readingCardLocalForward = transform.InverseTransformVector(m_attachedPosition.forward);
        
    }

    private void Success()
    {
        CancelErrorBlinking();
        //StartCoroutine(Blink(m_successLamp.material));
        m_successLamp.material.EnableKeyword("_EMISSION");
       
        //m_successLamp.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        RemoveListeners();
    }
    private void Error()
    {
        CancelErrorBlinking();
        m_blinking = StartCoroutine(Blink(m_errorLamp.material));
    }
    private void RemoveListeners()
    {
        m_success.RemoveListener(Success);
        m_error.RemoveListener(Error);
    }


    private IEnumerator Blink(Material mat)
    {
        mat.EnableKeyword("_EMISSION");
        //mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

        yield return new WaitForSeconds(m_blinkTime);

        mat.DisableKeyword("_EMISSION");
        //mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        m_blinking = null;
    }
    private void CancelErrorBlinking()
    {
        if (m_blinking != null) StopCoroutine(m_blinking);
        m_blinking = null;
        //m_successLamp.material.DisableKeyword("_EMISSION");
        //m_successLamp.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        m_errorLamp.material.DisableKeyword("_EMISSION");
        m_errorLamp.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        
    }
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return false;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);


        m_card = args.interactableObject.transform.gameObject;
        m_cardGrabInteractable = m_card.GetComponent<XRGrabInteractable>();
        m_cardInCardReader = true;

        ResetMove();

    }
    private void UpdateCurrentController()
    {
        if (m_cardGrabInteractable.interactorsSelecting.Count > 0)
        {
            var controllerInteractor = m_cardGrabInteractable.interactorsSelecting[0] as XRBaseControllerInteractor;
            if (controllerInteractor != null) m_currentController = controllerInteractor.xrController;
        }
        else
        {
            m_currentController = null;
        }
    }
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        CancelInteractionWithSocket();

    }

    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractor(updatePhase);
        if(m_cardInCardReader)
        {
            if(updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                UpdateCurrentController();
                SwipeToCardReader();
                CheckSwipe();
                CheckDistanceToHand();
            }
        }
        


    }
    
    private void SwipeToCardReader()
    {
        
        m_cardGrabInteractable.trackPosition = false;
        m_cardGrabInteractable.trackRotation = false;
       // Debug.Log(m_cardGrabInteractable.trackPosition);
        m_card.transform.rotation = m_attachedPosition.rotation;
        if (m_currentController != null)
        {
            var relativeStartPosition = transform.InverseTransformPoint(m_currentController.transform.position);
            m_updatedrelativePosition = new Vector3(m_attachedRelativeX, m_attachedRelativeY, relativeStartPosition.z);
            var globalPosition = transform.TransformPoint(m_updatedrelativePosition);
            m_card.transform.position = globalPosition;
        }
        else
        {
            var relativeStartPosition = transform.InverseTransformPoint(m_card.transform.position);
            m_updatedrelativePosition = new Vector3(m_attachedRelativeX, m_attachedRelativeY, relativeStartPosition.z);
            var globalPosition = transform.TransformPoint(m_updatedrelativePosition);
            m_card.transform.position = globalPosition;
            
        }
    }
    private void CheckDistanceToHand()
    {
        if(m_currentController!=null && Vector3.Distance(m_card.transform.position, m_currentController.transform.position)> m_minDistanceToSocket)
        {
            m_cardGrabInteractable.trackPosition = true;
            m_cardGrabInteractable.trackRotation = true;

            //Debug.Log(m_cardGrabInteractable.trackPosition);
        }
    }
    private void CancelInteractionWithSocket()
    {
        if (m_cardInCardReader)
        {
            m_cardGrabInteractable.trackPosition = true;
            m_cardGrabInteractable.trackRotation = true;

            m_card = null;
            m_cardGrabInteractable = null;
            m_cardInCardReader = false;
            m_currentController = null;
            m_prevLocalCardPosition = Vector3.zero;
            m_error.Invoke();

        }
    }    
    private void CheckSwipe()
    {
        if (m_prevLocalCardPosition != Vector3.zero)
        {
            Vector3 deltaMove = m_updatedrelativePosition - m_prevLocalCardPosition;
            var moveDot = Vector3.Dot(deltaMove, m_readingCardLocalForward);
            if(moveDot>0)
            {
                CancelErrorBlinking();
                m_movingDistance += deltaMove.magnitude;
                
                
                if(m_movingDistance>= m_minMoveDistance)
                {
                    var speed = m_movingDistance / (Time.time - m_startMovingTime);
                    
                    if(speed>= m_minSpeed && speed<=m_maxSpeed)
                    {
                        m_success.Invoke();
                    }                     
                    else
                    {
                        m_error.Invoke();
;                   }
                }
            }
            else if (moveDot < 0)
            {
                m_error.Invoke();
                ResetMove();
            }
        }

        m_prevLocalCardPosition = m_updatedrelativePosition;
    }
    private void ResetMove()
    {
        m_startMovingTime = Time.time;
        m_movingDistance = 0f;
    }
}
