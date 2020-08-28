
using UnityEngine;
using UnityEngine.UIElements;
using BlueGraph;
using BlueGraph.Editor;

namespace BlueGraphSamples.Editor
{
    /// <summary>
    /// Adds a preview texture of the Heightmap output `result`
    /// </summary>
    [CustomNodeView(typeof(HeightmapTransform))]
    [CustomNodeView(typeof(HeightmapFactory))]
    class HeightmapPreviewNodeView : NodeView
    {
        Texture2D previewTexture;
        bool error;

        const int TEXTURE_SIZE = 256;

        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/HeightmapPreviewNodeView"));
            AddToClassList("heightmapPreviewNodeView");
            
            // Setup a default texture preview
            previewTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            var preview = new VisualElement();
            preview.style.backgroundImage = new StyleBackground(previewTexture);
            preview.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            
            preview.style.minWidth = TEXTURE_SIZE;
            preview.style.minHeight = TEXTURE_SIZE;

            extensionContainer.Add(preview);
            RefreshExpandedState();
        }

        protected override void OnDestroy()
        {
            /*if (target is HeightmapTransform t)
            {
                t.onUpdateResult -= UpdateTexture;
            }
            else if (target is HeightmapFactory f)
            {
                f.onUpdateResult -= UpdateTexture;
            }*/
            
            base.OnDestroy();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Re-evaluate and generate new preview textures
            if (Target is HeightmapTransform transform)
            {
                transform.DirtyResult();
                UpdateTexture(transform.GetPort("result").GetValue<Heightmap>());
            }
            else if (Target is HeightmapFactory factory)
            {
                factory.DirtyResult();
                UpdateTexture(factory.GetPort("result").GetValue<Heightmap>());
            }
        }

        /// <summary>
        /// Generate a 2D texture preview of the heightmap for debugging
        /// </summary>
        /// <param name="map"></param>
        protected void UpdateTexture(Heightmap map)
        {
            // TODO: Some sort of prettier "NO TEXTURE" output?
            if (map == null)
            {
                return;
            }

            Color high = Color.red;
            Color mid = Color.yellow;
            Color low = Color.green;

            Color[] c = previewTexture.GetPixels();
            float size = (float)TEXTURE_SIZE;

            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    float height = map.GetHeightBilinear(x / size, y / size);

                    c[TEXTURE_SIZE * y + x] = height > 0.5f ? 
                        Color.Lerp(mid, high, (height - 0.5f) * 2f) : 
                        Color.Lerp(low, mid, height * 2f);
                }
            }

            previewTexture.SetPixels(c);
            previewTexture.Apply();
        }
    }
}
