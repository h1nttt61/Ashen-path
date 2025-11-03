using System;
using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public SceneFader sceneFader;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            try
            {
                sceneFader.TriggerSceneChange();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Couldn't fade to the next scene " + ex);
            }
        }
    }
}
