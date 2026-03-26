using UnityEngine;

[System.Serializable]
public struct AccelerometerStuffStruct {
    public float timestamp;
    public float deltaTime;
    public Vector3 values;

    public Vector3 pos1;
    public Vector3 pos2;
    public Vector3 gforce;

    public ConstantsMovements movementType;

    public AccelerometerStuffStruct(float timestamp, float deltaTime, Vector3 values, Vector3 pos1, Vector3 pos2, Vector3 gforce, ConstantsMovements movementType) {
        this.timestamp = timestamp;
        this.deltaTime = deltaTime;
        this.values = values;
        this.pos1 = pos1;
        this.pos2 = pos2;
        this.gforce = gforce;
        this.movementType = movementType;
    }
}

[System.Serializable]
public struct DeltaPositionsStruct {
    public float timestamp;
    public float deltaTime;
    public Vector3 values;
    public Vector3 gForce;
    public ConstantsMovements movementType;

    public DeltaPositionsStruct(float timestamp, float deltaTime, Vector3 values, Vector3 gForce, ConstantsMovements movementType) {
        this.timestamp = timestamp;
        this.deltaTime = deltaTime;
        this.values = values;
        this.gForce = gForce;
        this.movementType = movementType;
    }
}

public enum ConstantsMovements {
    idle,
    fall,
    walking,
    after_fall,
    transition,
    animTransitionWalk,
    notFall
}

public static class ConstantsMovementsExtension {
    public static string ToAnimationString(this ConstantsMovements movement) {
        return movement switch {
            ConstantsMovements.idle => "Idle",
            ConstantsMovements.fall => "Fall",
            ConstantsMovements.walking => "Walking",
            ConstantsMovements.after_fall => "After_fall",
            ConstantsMovements.transition => "Transition",
            ConstantsMovements.animTransitionWalk => "TransitionWalk",
            ConstantsMovements.notFall => "NotFall",
            _ => ""
        };
    }
}
