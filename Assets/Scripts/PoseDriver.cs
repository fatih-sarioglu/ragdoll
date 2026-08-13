using System.Collections.Generic;
using UnityEngine;

public class PoseDriver : MonoBehaviour
{
    [SerializeField] Transform animatedRoot;
    [SerializeField] Transform ragdollRoot;

    // A ragdoll bone (driven by a ConfigurableJoint) matched to its
    // same-named counterpart in the animated hierarchy.
    public class BonePair
    {
        public Quaternion startLocalRotation;
        public ConfigurableJoint joint;
        public Transform ragdollBone;
        public Transform animatedBone;
    }

    readonly List<BonePair> _pairs = new List<BonePair>();

    void Awake()
    {
        // pair up the bones in the animated and ragdoll hierarchies
        BuildPairs();
    }

    void BuildPairs()
    {
        _pairs.Clear();

        if (animatedRoot == null || ragdollRoot == null)
        {
            Debug.LogError("PoseDriver: animatedRoot and ragdollRoot must be assigned.", this);
            return;
        }

        // Build a name -> Transform lookup for the animated hierarchy so we can
        // resolve each ragdoll bone by its (matching) name in O(1).
        var animatedByName = new Dictionary<string, Transform>();
        foreach (Transform bone in animatedRoot.GetComponentsInChildren<Transform>())
        {
            if (animatedByName.ContainsKey(bone.name))
            {
                Debug.LogWarning($"PoseDriver: duplicate animated bone name '{bone.name}'. " +
                                 "Pairing may be ambiguous.", bone);
                continue;
            }
            animatedByName.Add(bone.name, bone);
        }

        // The ConfigurableJoints define which ragdoll bones are drivable.
        foreach (ConfigurableJoint joint in ragdollRoot.GetComponentsInChildren<ConfigurableJoint>())
        {
            Transform ragdollBone = joint.transform;

            if (!animatedByName.TryGetValue(ragdollBone.name, out Transform animatedBone))
            {
                Debug.LogWarning($"PoseDriver: no animated bone named '{ragdollBone.name}' " +
                                 "to pair with ragdoll joint.", ragdollBone);
                continue;
            }

            _pairs.Add(new BonePair
            {
                startLocalRotation = ragdollBone.localRotation,
                joint = joint,
                ragdollBone = ragdollBone,
                animatedBone = animatedBone,
            });
        }

        Debug.Log($"PoseDriver: paired {_pairs.Count} bone(s).", this);
    }

    void FixedUpdate()
    {
        // drive each ragdoll bone toward its animated counterpart
        foreach (BonePair pair in _pairs)
        {
            pair.joint.SetTargetRotationLocal(pair.animatedBone.localRotation, pair.startLocalRotation);
        }
    }
}
