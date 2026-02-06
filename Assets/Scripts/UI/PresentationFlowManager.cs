using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class PresentationFlowManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> sequenceElementObjects;
    private Sequence mainFlowSequence;
    private Sequence activeStepSequence;
    private bool presentationFinished = false;
    private Action _onStopped;

    void Awake()
    {
        InitializeAllElements();
    }

    void OnDisable()
    {
        if (mainFlowSequence != null && mainFlowSequence.IsActive())
            mainFlowSequence.Kill();
        mainFlowSequence = null;
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

    public void StartPresentation(Action onCompleted, Action onStopped = null)
    {
        InitializeAllElements();
        presentationFinished = false;
        _onStopped = onStopped;

        if (mainFlowSequence != null && mainFlowSequence.IsActive())
            mainFlowSequence.Kill();
        mainFlowSequence = null;

        mainFlowSequence = DOTween.Sequence();
        mainFlowSequence.SetAutoKill(false);
        mainFlowSequence.OnKill(() =>
        {
            mainFlowSequence = null;
            _onStopped?.Invoke();
            _onStopped = null;
        });

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
            _onStopped = null;
            mainFlowSequence = null;
            onCompleted?.Invoke();
        });

        mainFlowSequence.Play();
    }

    public void StopPresentation()
    {
        if (mainFlowSequence != null && mainFlowSequence.IsActive())
        {
            mainFlowSequence.Kill();
        }
        mainFlowSequence = null;
        InitializeAllElements();
        Debug.Log("Presentation stopped.");
    }

    public bool IsPresentationFinished()
    {
        return presentationFinished;
    }
}