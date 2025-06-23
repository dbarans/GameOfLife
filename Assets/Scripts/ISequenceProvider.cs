using DG.Tweening;
using UnityEngine;

public interface ISequenceProvider
{
    void InitializeState();
    Sequence GetAnimationSequence();
}