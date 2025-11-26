using System;
using System.Collections.Generic;
using UnityEngine;

public class BodyMapper : MonoBehaviour {

    [SerializeField] private List<Transform> bodyPartsFrom;
    [SerializeField] private List<Transform> bodyPartsTo;

    [SerializeField] private List<BodyPartPair> bodyPartPairs;

    [SerializeField] private BodyPartPair rootPair;

    private void Update() {
        if (bodyPartPairs == null || bodyPartPairs.Count == 0) return;

        rootPair.AdjustTransform(local: false);

        foreach (var pair in bodyPartPairs) {
            pair.AdjustTransform();
        }
    }

    [ContextMenu("Set Body Part Pairs")]
    public void CreatePairs() {
        if (bodyPartsFrom == null || bodyPartsTo == null ||
            bodyPartsFrom.Count != bodyPartsTo.Count) {
            Debug.LogWarning("Body parts lists are not set or do not match in size.");
            return;
        }
        rootPair = new BodyPartPair(
            bodyPartsFrom[0],
            bodyPartsTo[0]
        );
        rootPair.CalculateCurrentBodyPartOffsets(local: false);
        bodyPartPairs = new List<BodyPartPair>();
        for (int i = 1; i < bodyPartsFrom.Count; i++) {
            var pair = new BodyPartPair(
                bodyPartsFrom[i],
                bodyPartsTo[i]
            );
            pair.CalculateCurrentBodyPartOffsets();
            bodyPartPairs.Add(pair);
        }
    }

    [ContextMenu("Reset Body Part Offsets")]
    public void SafeOffsets() {
        if (bodyPartPairs == null || bodyPartPairs.Count == 0) return;

        rootPair.CalculateCurrentBodyPartOffsets(local: false);

        foreach (var pair in bodyPartPairs) {
            pair.CalculateCurrentBodyPartOffsets();
        }
    }

}

[Serializable]
public class BodyPartPair {
    [field: SerializeField] public Transform From { get; private set; }

    [field: SerializeField] public Transform To { get; private set; }

    [field: SerializeField] public Vector3 TranslationOffset { get; private set; }

    [field: SerializeField] public Quaternion RotationOffset { get; private set; }

    public BodyPartPair(Transform from, Transform to) {
        From = from;
        To = to;
    }

    public void CalculateCurrentBodyPartOffsets(bool local = true) {
        if (From != null && To != null) {
            TranslationOffset = local ? To.localPosition - From.localPosition
                                      : To.position - From.position;

            RotationOffset = local ? Quaternion.FromToRotation(From.localEulerAngles, To.localEulerAngles)
                                   : Quaternion.FromToRotation(From.eulerAngles, To.eulerAngles);
        } else {
            Debug.LogWarning("One of the body parts is null, cannot save offsets.");
        }
    }

    public void AdjustTransform(bool local = true) {
        if (From != null && To != null) {
            if (local) {
                From.localPosition = To.localPosition - TranslationOffset;
                From.localRotation = To.localRotation * Quaternion.Inverse(RotationOffset);
            } else {
                From.position = To.position - TranslationOffset;
                From.rotation = To.rotation * Quaternion.Inverse(RotationOffset);
            }
        }
    }
}