using UnityEngine;

public class PopNotes : MonoBehaviour
{
    [SerializeField] private ParticleSystem noteParticle;
    private BoxCollider boxCol;

    private void Start()
    {
        if (noteParticle == null)
            noteParticle = GetComponentInChildren<ParticleSystem>();

        if (noteParticle != null)
            noteParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        boxCol = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (noteParticle != null)
                noteParticle.Play();

            Debug.Log("플레이어 들어옴!");
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (noteParticle != null)
                noteParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            Debug.Log("플레이어 나감!");
        }
    }
}