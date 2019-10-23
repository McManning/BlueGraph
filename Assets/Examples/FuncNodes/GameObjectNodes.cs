using UnityEngine;
using BlueGraph;

namespace BlueGraphExamples.FuncNodes
{
    /// <summary>
    /// Suite of func nodes based around GO data
    /// </summary>
    [FuncNodeModule("Unity/GameObject")]
    public static class GameObjectNodes
    {
        public static Transform GetTransform(GameObject go) => go.transform;
        public static Vector3 GetWorldPosition(GameObject go) => go.transform.position;
        public static Vector3 GetLocalPosition(GameObject go) => go.transform.localPosition;

        // Downcasted to Object to test inheritance
        public static string GetName(UnityEngine.Object obj) => obj.name;
        
        // Setters should have an execution line, I'd think. Otherwise it's risky as to *when* they can be set.
        //public static void SetPosition(GameObject go, Vector3 position) => go.transform.position = position;
        //public static void SetLocalPosition(GameObject go, Vector3 position) => go.transform.localPosition = position;

        // TODO: Rename-able default output slot...
        public static Vector3 BreakTransform(Transform t, out Quaternion rot, out Vector3 scale)
        {
            rot = t.rotation;
            scale = t.lossyScale;
            return t.position;
        }
    }
}
