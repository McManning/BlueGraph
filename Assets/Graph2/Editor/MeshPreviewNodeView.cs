using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graph2
{
    [CustomNodeView(typeof(MeshPreviewTestNode))]
    class MeshPreviewNodeView : NodeView
    {
        MeshPreviewTestNode target;
        PreviewRenderUtility m_PreviewUtility;
    
        Vector3 m_PreviewEuler = new Vector3(45f, 0, 0);

        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);

            m_PreviewUtility = new PreviewRenderUtility();

            target = node as MeshPreviewTestNode;

            var container = new IMGUIContainer(() =>
            {
                OnGUI();
            });

            var slider = new Slider(0f, 180f, SliderDirection.Horizontal);
            slider.RegisterValueChangedCallback(OnSliderChange);
            
            extensionContainer.Add(slider);
            extensionContainer.Add(container);

            RefreshExpandedState();

            Debug.Log("Initialize custom editor with comment support");
        }

        private void OnSliderChange(ChangeEvent<float> change)
        {
            m_PreviewEuler.y = change.newValue;
        }

        private void OnGUI()
        {
            Rect layoutRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(200f));
         
            GUILayout.FlexibleSpace();
            
            Rect r = GUILayoutUtility.GetLastRect();
            r.height = r.width;

            GUI.DrawTexture(r, RenderPreview(r));

            EditorGUILayout.EndHorizontal();

            // Draw it onto the UIElement somehow.

            var evt = Event.current;
            
            
            // do stuff
        }

        private Texture RenderPreview(Rect r)
        {
            m_PreviewUtility.BeginPreview(r, null);

            // Render to the preview utility
            m_PreviewUtility.camera.backgroundColor = Color.black;
            m_PreviewUtility.camera.clearFlags = CameraClearFlags.Color;
            
            if (target.mesh != null)
            {                
                // Adjust the mesh position to fit the camera
                var bounds = target.mesh.bounds;
                var mag = bounds.extents.magnitude;
                var distance = 10f * mag;

                Debug.Log(mag + " - " + distance);
                
                // Fixed camera position some distance from the model and tilted
                m_PreviewUtility.camera.transform.position = new Vector3(0, 0, -distance);
                m_PreviewUtility.camera.transform.rotation = Quaternion.identity;

                m_PreviewUtility.camera.nearClipPlane = 0.1f;
                m_PreviewUtility.camera.farClipPlane = 100f; // distance + mag * 1.1f;

                Quaternion rot = Quaternion.Euler(m_PreviewEuler);
                Vector3 pos = Vector3.zero; // rot * -bounds.center;
                
                m_PreviewUtility.DrawMesh(target.mesh, pos, rot, target.material, 0);
            }

            // Render the camera view and generate the render texture
            m_PreviewUtility.camera.Render();
            Texture image = m_PreviewUtility.EndPreview();

            return image;
        }
    }
}
