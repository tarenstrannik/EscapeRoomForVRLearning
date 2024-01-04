using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TouchButton : XRBaseInteractable
{
    [Header("Button options")]
    [SerializeField] private int m_buttonValue;
    [SerializeField] private Material m_buttonHoveredMaterial;
    private Material m_defaultButtonMaterial;
    private Renderer m_buttonRenderer;


    private XRBaseController m_currentController;
    private Vector3 m_startPosition;
    [SerializeField] private float m_deltaMovement = 0.02f;
    private float m_startControllerPositionRelativeToKeypadY;
    private bool m_isButtonPressed = false;
    public UnityEvent<int> m_pressedEvent;
    public UnityEvent<int> m_releasedEvent;

    protected override void Awake()
    {
        base.Awake();
        m_buttonRenderer = GetComponentInChildren<Renderer>();
        m_defaultButtonMaterial = m_buttonRenderer.material;
        m_startPosition = transform.parent.transform.InverseTransformPoint(transform.position);
        m_releasedEvent.AddListener(ResetButton);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //base.OnSelectEntered(args);
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        //base.OnSelectExited(args);
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        if (m_currentController == null)
        {
            ChangeButtonColorOnHover(true);
        
            var controllerInteractor = args.interactorObject as XRBaseControllerInteractor;
            m_currentController = controllerInteractor.xrController;
            m_startControllerPositionRelativeToKeypadY = transform.parent.transform.InverseTransformPoint(m_currentController.transform.position).y;
        }

    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        var controllerInteractor = args.interactorObject as XRBaseControllerInteractor;
        if (m_currentController == controllerInteractor.xrController)
        {
            ChangeButtonColorOnHover(false);
            ResetButton(m_buttonValue);
            m_currentController = null;
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if(updatePhase== 0 && m_currentController != null)
        {
            MoveButton();
        }
    }

    private void MoveButton()
    {
        float curControllerY= transform.parent.transform.InverseTransformPoint(m_currentController.transform.position).y;
        
        float deltaY = m_startControllerPositionRelativeToKeypadY - curControllerY;
        if (deltaY > 0 && deltaY < m_deltaMovement)
        {
            Vector3 curLocalPosition = new Vector3(m_startPosition.x, m_startPosition.y - deltaY, m_startPosition.z);
            transform.position = transform.parent.TransformPoint(curLocalPosition);
        }
        else if(deltaY <=0)
        {
            m_releasedEvent.Invoke(m_buttonValue);
        }
        else if(!m_isButtonPressed)
        {
            m_isButtonPressed = true;
            m_pressedEvent.Invoke(m_buttonValue);
        }
    }
    private void ChangeButtonColorOnHover(bool change)
    {
        if(change)
        {
            m_buttonRenderer.material = m_buttonHoveredMaterial;
        }
        else
        {
            m_buttonRenderer.material = m_defaultButtonMaterial;
        }
    }

    private void ResetButton(int value)
    {
        m_isButtonPressed = false;
        transform.position = transform.parent.TransformPoint(m_startPosition);
    }
}
