using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
public class NumberPadDisplay : MonoBehaviour
{
    [SerializeField] private NumberPad m_linkedNumberpad;
    [SerializeField] private TextMeshProUGUI m_captionText;
    [SerializeField] private TextMeshProUGUI[] m_enteredCodeText;
    [SerializeField] private Image m_screenImage;
    [SerializeField] private Color m_successColor;
    [SerializeField] private Color m_errorColor;
    [SerializeField] private Color m_defaultColor;

    [SerializeField] private string m_defaultChar;
    [SerializeField] private string m_defaultCaption;
    [SerializeField] private string m_enteringCaption;
    [SerializeField] private string m_successCaption;
    [SerializeField] private string m_errorCaption;

    [SerializeField] private GameObject m_keyCardPrefab;
    [SerializeField] private Transform m_keyCardInstantiatingPosition;

    private int m_curNumberPosition = 0;

    private Coroutine m_errorCoroutine = null;
    [SerializeField] private float m_errorScreenDelay=0.5f;


    
    private void Awake()
    {
        ResetScreen();
    }

    private void Start()
    {
        m_linkedNumberpad.m_touchpadButtonPressedEvent.AddListener(EnterOneNumber);
        m_linkedNumberpad.m_success.AddListener(Success);
        m_linkedNumberpad.m_error.AddListener(Error);
    }



    private void EnterOneNumber(int number)
    {
        if (m_errorCoroutine != null) ResetScreen();
        if (m_captionText.text != m_enteringCaption) m_captionText.SetText(m_enteringCaption);
        m_enteredCodeText[m_curNumberPosition].SetText(number.ToString());
        m_curNumberPosition++;
    }
    private void Success()
    {
        m_screenImage.color = m_successColor;
        m_captionText.text = m_successCaption;
        m_linkedNumberpad.m_touchpadButtonPressedEvent.RemoveListener(EnterOneNumber);
        m_linkedNumberpad.m_success.RemoveListener(Success);
        m_linkedNumberpad.m_error.RemoveListener(Error);
        Instantiate(m_keyCardPrefab, m_keyCardInstantiatingPosition);
    }
    
    private void Error()
    {
        
        m_screenImage.color = m_errorColor;
        m_captionText.text = m_errorCaption;
        m_curNumberPosition = 0;

        m_errorCoroutine = StartCoroutine(IErrorCoroutine());
    }
    private IEnumerator IErrorCoroutine()
    {
        yield return new WaitForSeconds(m_errorScreenDelay);
        ResetScreen();
    }
    private void ResetScreen()
    {
        if (m_errorCoroutine != null) StopCoroutine(m_errorCoroutine);
        m_errorCoroutine = null;
        m_screenImage.color= m_defaultColor;
        foreach(var text in m_enteredCodeText)
        {
            text.SetText(m_defaultChar);
        }
        m_captionText.text = m_defaultCaption;
    }

}
