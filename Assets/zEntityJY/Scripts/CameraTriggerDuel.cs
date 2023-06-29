using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class CameraTriggerDuel : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    private CinemachineTargetGroup targetScript;

    private void Start() {
        
    }

    private void OnCollisionEnter2D(Collision2D other) {
        GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>().m_Targets[0].target = player1;
        GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>().m_Targets[1].target = player2;
    }

    private void OnCollisionExit2D(Collision2D other) {
        GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>().m_Targets[0].target = player1;
        GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>().m_Targets[1].target = null;
    }
}
