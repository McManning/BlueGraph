
using BlueGraph;
using BlueGraphEditor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlueGraphExamples
{
    /// <summary>
    /// Variant of node rendering where we just render 
    /// an icon in the center of the node
    /// </summary>
    [CustomNodeView(typeof(IconNode))]
    class IconNodeView : NodeView
    {
        // TODO: It makes sense just to add this behavior to the base node view
        // and let the developer define templates to use for icons. 
        public override void Initialize(AbstractNode node, EdgeConnectorListener connectorListener)
        {
            base.Initialize(node, connectorListener);
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/IconNodeView"));
            AddToClassList("iconNodeView");
            
            VisualElement iconContainer = new VisualElement
            {
                name = "icon"
            };

            string iconName = "Icons/Add";
            foreach (var attr in node.GetType().GetCustomAttributes(false))
            {
                if (attr is NodeIconAttribute iconAttr)
                {
                    iconName = iconAttr.Name;
                    break;
                }
            }
            
            var icon = Resources.Load<Texture2D>(iconName);

            iconContainer.style.backgroundImage = icon;

            inputContainer.parent.Add(iconContainer);
            iconContainer.PlaceInFront(inputContainer);

            // TODO: Tooltip support on the node body
        }
    }
}
