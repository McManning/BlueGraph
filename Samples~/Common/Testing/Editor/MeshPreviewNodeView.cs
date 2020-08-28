
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples
{
    /// <summary>
    /// This is an example of how to display a scene preview from inside a node
    /// 
    /// Use cases:
    /// * Previewing the steps of blending procedural content generation
    /// * Showing a material preview that's dynamically updated along with the node
    /// </summary>
    [CustomNodeView(typeof(MeshPreview))]
    class MeshPreviewNodeView : NodeView
    {
        private PreviewRenderUtility previewRenderUtility;
        private Vector3 previewEuler = new Vector3(30f, 0, 0);

        protected override void OnInitialize()
        {
            previewRenderUtility = new PreviewRenderUtility();
            
            // Add a slider to rotate the preview mesh
            var slider = new Slider(0f, 360f, SliderDirection.Horizontal);
            slider.RegisterValueChangedCallback(OnSliderChange);
            
            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);

            extensionContainer.Add(slider);
            extensionContainer.Add(container);

            // Currently needed by Unity's base Node class to properly
            // resize for extensionContent. Might be a bug.
            RefreshExpandedState();
        }

        protected override void OnDestroy()
        {
            // Avoid leaking in the editor by cleaning up the preview component
            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup();
                previewRenderUtility = null;
            }

            base.OnDestroy();
        }
        
        private void OnSliderChange(ChangeEvent<float> change)
        {
            previewEuler.y = change.newValue;
        }
        
        /// <summary>
        /// Render the texture preview on GUI updates
        /// </summary>
        private void OnGUI()
        {
            var layoutRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(200));
            
            if (layoutRect.width > 0)
            {
                var r = new Rect(0, 0, layoutRect.width, 200);
                GUI.DrawTexture(r, RenderPreview(r));
            }
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Setup and draw to a mini scene with a RenderTexture output
        /// </summary>
        private Texture RenderPreview(Rect r)
        {
            var node = Target as MeshPreview;

            previewRenderUtility.BeginPreview(r, null);
            
            previewRenderUtility.camera.backgroundColor = Color.black;
            previewRenderUtility.camera.clearFlags = CameraClearFlags.Color;
            
            if (node.mesh != null)
            {                
                // Adjust the mesh position to fit to the viewport
                // Reference: https://gist.github.com/radiatoryang/a2282d44ba71848e498bb2e03da98991
                var bounds = node.mesh.bounds;
                var magnitude = bounds.extents.magnitude;
                var distance = 10f * magnitude;
                
                previewRenderUtility.camera.transform.position = new Vector3(0, 0, -distance);
                previewRenderUtility.camera.transform.rotation = Quaternion.identity;

                previewRenderUtility.camera.nearClipPlane = 0.1f;
                previewRenderUtility.camera.farClipPlane = distance + magnitude * 1.1f;

                var rot = Quaternion.Euler(previewEuler);
                var pos = rot * -bounds.center;
                
                previewRenderUtility.DrawMesh(node.mesh, pos, rot, node.material, 0);
            }

            // Render the camera view and generate the render texture
            previewRenderUtility.camera.Render();
            return previewRenderUtility.EndPreview();
        }
    }
}
