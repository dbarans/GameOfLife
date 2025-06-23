using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PresentationFlowManager : MonoBehaviour
{
    [Header("Sequence Elements")]
    [SerializeField] private List<GameObject> sequenceElementObjects;

    [Header("Interaction Settings")]
    [SerializeField] private float animationSpeedUpFactor = 2.0f;

    private Sequence mainFlowSequence;
    private Sequence activeStepSequence;

    void Awake()
    {
        InitializeAllElements();
    }
    private void Start()
    {
        StartPresentationFlow();
    }

    void OnDisable()
    {
        mainFlowSequence?.Kill();
        DOTween.KillAll();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (activeStepSequence != null && activeStepSequence.IsPlaying())
            {
                if (activeStepSequence.timeScale == 1.0f)
                {
                    activeStepSequence.timeScale = animationSpeedUpFactor;
                }
                else
                {
                    activeStepSequence.timeScale = 1.0f;
                }
            }
            else if (mainFlowSequence != null && mainFlowSequence.IsPlaying())
            {
                if (mainFlowSequence.timeScale == 1.0f)
                {
                    mainFlowSequence.timeScale = animationSpeedUpFactor;
                }
                else
                {
                    mainFlowSequence.timeScale = 1.0f;
                }
            }
        }
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

    private void StartPresentationFlow()
    {
        mainFlowSequence = DOTween.Sequence();
        mainFlowSequence.SetAutoKill(false);

        if (sequenceElementObjects == null || sequenceElementObjects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < sequenceElementObjects.Count; i++)
        {
            GameObject go = sequenceElementObjects[i];
            if (go == null)
            {
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

        mainFlowSequence.AppendCallback(() => Debug.Log("Presentation finished!"));
        mainFlowSequence.Play();
    }
}
