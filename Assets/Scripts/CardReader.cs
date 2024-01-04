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

    public UnityEvent m_success;
    public UnityEvent m_error;


    private Coroutine m_blinking;
    protected override void Awake()
    {
        m_success.AddListener(Success);
        m_error.AddListener(Error);
    }

    private void Success()
    {
        if (m_blinking != null) CancelBlinking();
        m_blinking = StartCoroutine(Blink(m_successLamp.material));
        RemoveListeners();
    }
    private void Error()
    {
        if (m_blinking != null) CancelBlinking();
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
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

        yield return new WaitForSeconds(m_blinkTime);

        mat.DisableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        m_blinking = null;
    }
    private void CancelBlinking()
    {
        StopCoroutine(m_blinking);
        m_blinking = null;
        m_successLamp.material.DisableKeyword("_EMISSION");
        m_successLamp.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        m_errorLamp.material.DisableKeyword("_EMISSION");
        m_errorLamp.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    }
}
