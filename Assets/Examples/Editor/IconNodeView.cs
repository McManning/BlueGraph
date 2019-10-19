
using BlueGraph;
using BlueGraphEditor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlueGraphExamples
{
    /// <summary>
    /// Different style of a node where the title is (optionally)
    /// rendered in the center with a backing icon. Very UE4, basically.
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

            string iconName = null;
            bool showTitle = true;
            foreach (var attr in node.GetType().GetCustomAttributes(false))
            {
                if (attr is NodeIconAttribute iconAttr)
                {
                    iconName = iconAttr.iconName;
                    showTitle = iconAttr.showTitle;
                    break;
                }
            }
            
            if (showTitle)
            {
                var titleLabel = new Label();
                titleLabel.text = title;
                titleLabel.AddToClassList("iconNodeTitle");

                iconContainer.Add(titleLabel);
            }
            
            if (iconName != null)
            {
                var icon = Resources.Load<Texture2D>(iconName);
                iconContainer.style.backgroundImage = icon;
            }
            
            inputContainer.parent.Add(iconContainer);
            iconContainer.PlaceInFront(inputContainer);
        }
    }
}
