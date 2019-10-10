
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using BlueGraph;
using BlueGraphEditor;

namespace BlueGraphExamples
{
    /// <summary>
    /// This is an example of how to construct a preview scene within an IMGUIContainer
    /// inside of a node to preview a mesh, prefab, etc.
    /// 
    /// Use cases:
    /// * Previewing the steps of blending procedural content generation
    /// * Showing a material preview that's dynamically updated along with the node
    /// </summary>
    [CustomNodeView(typeof(MeshPreviewTestNode))]
    class MeshPreviewNodeView : NodeView
    {
        MeshPreviewTestNode m_target;
        PreviewRenderUtility m_PreviewUtility;
        
        Vector3 m_PreviewEuler = new Vector3(45f, 0, 0);

        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);
            m_target = node as MeshPreviewTestNode;
            
            m_PreviewUtility = new PreviewRenderUtility();
            
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
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }

            base.OnDestroy();
        }
        
        private void OnSliderChange(ChangeEvent<float> change)
        {
            m_PreviewEuler.y = change.newValue;
        }
        
        private void OnGUI()
        {
            var layoutRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(200));
            
            if (layoutRect.width > 0)
            {
                // TODO: A more optimal solution would be to only re-render when something changes.
                // E.g. they change the rotation slider, connections change, values change, etc. 
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
            m_PreviewUtility.BeginPreview(r, null);
            
            m_PreviewUtility.camera.backgroundColor = Color.black;
            m_PreviewUtility.camera.clearFlags = CameraClearFlags.Color;
            
            if (m_target.mesh != null)
            {                
                // Adjust the mesh position to fit to the viewport
                // Reference: https://gist.github.com/radiatoryang/a2282d44ba71848e498bb2e03da98991
                var bounds = m_target.mesh.bounds;
                var magnitude = bounds.extents.magnitude;
                var distance = 10f * magnitude;
                
                m_PreviewUtility.camera.transform.position = new Vector3(0, 0, -distance);
                m_PreviewUtility.camera.transform.rotation = Quaternion.identity;

                m_PreviewUtility.camera.nearClipPlane = 0.1f;
                m_PreviewUtility.camera.farClipPlane = distance + magnitude * 1.1f;

                var rot = Quaternion.Euler(m_PreviewEuler);
                var pos = rot * -bounds.center;
                
                m_PreviewUtility.DrawMesh(m_target.mesh, pos, rot, m_target.material, 0);
            }

            // Render the camera view and generate the render texture
            m_PreviewUtility.camera.Render();
            return m_PreviewUtility.EndPreview();
        }
    }
}
