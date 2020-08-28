using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlueGraphSamples
{
    /// <summary>
    /// Attach to an NPC to interact with it.
    /// 
    /// On interaction, the DialogGraph will be executed to play
    /// out the conversation the player is having with that NPC.
    /// </summary>
    public class Dialog : MonoBehaviour
    {
        public DialogGraph graph;
        
        /// <summary>
        /// DialogUI Prefab. Here for laziness. 
        /// </summary>
        public GameObject uiPrefab;

        IEnumerator StartDialog()
        {
            // Add the dialog UI to the screen
            var canvas = GameObject.Find("Canvas");
            var instance = Instantiate(uiPrefab, canvas.transform);
            var ui = instance.GetComponent<DialogUI>();

            // Run the graph 
            yield return graph.Execute(ui);
            
            // And cleanup
            Destroy(instance);
            yield return null;
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (graph)
            {
                StartCoroutine(StartDialog());
            } 
            else
            {
                Debug.LogWarning("No DialogGraph set");
            }
        }
    }
}
