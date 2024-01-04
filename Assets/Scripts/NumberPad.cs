using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NumberPad : MonoBehaviour
{
    public UnityEvent<int> m_touchpadButtonPressedEvent;
    public UnityEvent m_success;
    public UnityEvent m_error;
    [SerializeField] private int m_correctCode = 1234;
    private int m_enteredCode=0;

    [SerializeField] private TouchButton[] m_buttons;

    private bool m_isNumberEntered = false;
    private void Awake()
    {
        foreach(TouchButton button in m_buttons)
        {
            button.m_pressedEvent.AddListener(ButtonPressed);
        }
        m_touchpadButtonPressedEvent.AddListener(AddNumberToEnteredCode);
        m_error.AddListener(ResetEnteredCode);
    }
    private void Update()
    {
        if(m_isNumberEntered)
        {
            CheckValidity();
        }
    }
    private void ButtonPressed(int buttonValue)
    {
        m_touchpadButtonPressedEvent.Invoke(buttonValue);
    }

    private void AddNumberToEnteredCode(int number)
    {
        m_enteredCode = m_enteredCode * 10 + number;
        m_isNumberEntered = true;
    }
    private void RemoveListeners()
    {
        foreach (TouchButton button in m_buttons)
        {
            button.m_pressedEvent.RemoveListener(ButtonPressed);
        }
        m_touchpadButtonPressedEvent.RemoveListener(AddNumberToEnteredCode);
        m_error.RemoveListener(ResetEnteredCode);
    }
    private void CheckValidity()
    {
        m_isNumberEntered = false;
        if (m_enteredCode== m_correctCode)
        {
            m_success.Invoke();
            RemoveListeners();
        }
        else if(SameLength(m_enteredCode, m_correctCode))
        {
            m_error.Invoke();
        }
    }

    private bool SameLength(int? number1, int? number2)
    {

        while (number1 > 0 && number2 > 0)
        {
            number1 = number1 / 10;
            number2 = number2 / 10;
        }

        // Both must be 0 now if
        // they had same lengths
        if (number1 == 0 && number2 == 0)
            return true;
        return false;
    }

    private void ResetEnteredCode()
    {
        m_enteredCode = 0;
    }
}
