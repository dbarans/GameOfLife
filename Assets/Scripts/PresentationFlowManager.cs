using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class PresentationFlowManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> sequenceElementObjects;

    [SerializeField] private float animationSpeedUpFactor = 2.0f;

    private Sequence mainFlowSequence;
    private Sequence activeStepSequence;
    private bool presentationFinished = false;

    void Awake()
    {
        InitializeAllElements();
    }

    void OnDisable()
    {
        mainFlowSequence?.Kill();
        DOTween.KillAll();
    }


    private void InitializeAllElements()
    {
        if (sequenceElementObjects == null || sequenceElementObjects.Count == 0)
        {
            Debug.LogWarning("Sequence Elements Objects list is empty or null! No sequences will play.");
            return;
        }

        foreach (GameObject go in sequenceElementObjects)
        {
            if (go == null)
            {
                Debug.LogWarning("One of the elements in Sequence Elements Objects list is null and will be skipped.");
                continue;
            }

            ISequenceProvider provider = go.GetComponent<ISequenceProvider>();
            if (provider != null)
            {
                provider.InitializeState();
            }
            else
            {
                Debug.LogWarning($"GameObject '{go.name}' in Sequence Elements Objects list does not have a component implementing ISequenceProvider and will be skipped.");
            }
        }
    }

    public void StartPresentation(Action onCompleted)
    {
        InitializeAllElements();
        presentationFinished = false;

        mainFlowSequence?.Kill();

        mainFlowSequence = DOTween.Sequence();
        mainFlowSequence.SetAutoKill(false);

        if (sequenceElementObjects == null || sequenceElementObjects.Count == 0)
        {
            Debug.LogWarning("No sequence elements to present. Calling onCompleted callback immediately.");
            onCompleted?.Invoke();
            return;
        }

        for (int i = 0; i < sequenceElementObjects.Count; i++)
        {
            GameObject go = sequenceElementObjects[i];
            if (go == null)
            {
                Debug.LogWarning($"GameObject at position {i} in sequenceElementObjects is null. Skipping.");
                continue;
            }

            ISequenceProvider provider = go.GetComponent<ISequenceProvider>();
            if (provider != null)
            {
                Sequence elementSequence = provider.GetAnimationSequence();

                mainFlowSequence.AppendCallback(() =>
                {
                    activeStepSequence = elementSequence;
                    if (activeStepSequence != null)
                    {
                        activeStepSequence.timeScale = 1.0f;
                        activeStepSequence.SetAutoKill(true);
                    }
                });

                mainFlowSequence.Append(elementSequence);
            }
            else
            {
                Debug.LogWarning($"GameObject '{go.name}' at position {i} does not have a component implementing ISequenceProvider. Skipping.");
            }
        }

        mainFlowSequence.AppendCallback(() =>
        {
            Debug.Log("Presentation finished!");
            presentationFinished = true;
            onCompleted?.Invoke();
        });

        mainFlowSequence.Play();
    }

    public void StopPresentation()
    {
        mainFlowSequence?.Kill();
        InitializeAllElements();
        Debug.Log("Presentation stopped.");
    }

    public bool IsPresentationFinished()
    {
        return presentationFinished;
    }
}