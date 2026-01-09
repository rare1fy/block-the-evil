using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    private Animator m_Animator;
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }
    
    public void PlayOverAnim()
    {
        m_Animator.Play("UIFightMain_Gameover_Close");
    }

    public void PlayReStartAnim()
    {
        m_Animator.Play("UIFightMain_Gameover_Open");
    }
}
