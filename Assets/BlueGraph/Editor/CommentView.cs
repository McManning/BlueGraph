
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using BlueGraph;

namespace BlueGraphEditor
{
    public class CommentView : GraphElement, ICanDirty
    {
        public enum Theme
        {
            Yellow,
            Grey,
            Red,
            Green,
            Blue
        }
        
        public NodeGroup target;
        public List<NodeView> containedNodes = new List<NodeView>();
        
        public Action<CommentView> onResize;

        Theme m_Theme;
        VisualElement m_MainContainer;
        VisualElement m_TitleContainer;
        TextField m_TitleEditor;
        Label m_TitleLabel;
        Vector2 m_Position;
        bool m_EditingCancelled;

        public CommentView(NodeGroup group)
        {
            target = group;
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/CommentView"));
            
            m_TitleContainer = new VisualElement();
            m_TitleContainer.AddToClassList("titleContainer");
            
            m_TitleEditor = new TextField();
            
            var input = m_TitleEditor.Q(TextField.textInputUssName);
            input.RegisterCallback<KeyDownEvent>(OnTitleKeyDown);
            input.RegisterCallback<FocusOutEvent>(e => { OnFinishEditingTitle(); });
            
            m_TitleContainer.Add(m_TitleEditor);
            
            m_TitleLabel = new Label();
            m_TitleLabel.text = group.title;
            
            m_TitleContainer.Add(m_TitleLabel);

            m_TitleEditor.style.display = DisplayStyle.None;

            Add(m_TitleContainer);

            ClearClassList();
            AddToClassList("commentView");
            
            capabilities |= Capabilities.Selectable | Capabilities.Movable | 
                            Capabilities.Deletable | Capabilities.Resizable;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<DetachFromPanelEvent>((e) => OnDestroy());

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            
            Enum.TryParse(target.theme, out m_Theme);
            SetTheme(m_Theme);
        }
        
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is CommentView)
            {
                // Add options to change theme
                foreach (var theme in (Theme[])Enum.GetValues(typeof(Theme)))
                {
                    evt.menu.AppendAction(
                        theme + " Theme", 
                        (a) => { SetTheme(theme); }, 
                        (m_Theme != theme) ? DropdownMenuAction.Status.Normal 
                            : DropdownMenuAction.Status.Disabled
                    );
                }

                evt.menu.AppendSeparator();
            }
        }
        
        public void SetTheme(Theme theme)
        {
            RemoveFromClassList("theme-" + m_Theme);
            AddToClassList("theme-" + theme);
            m_Theme = theme;
            target.theme = Enum.GetName(typeof(Theme), theme);
        }
        
        /// <summary>
        /// Executed when we're about to detach this element from the graph. 
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Dereference ourselves from contained nodes
            foreach (var node in containedNodes)
            {
                node.comment = null;
            }

            containedNodes.Clear();
        }
        
        private void OnTitleKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    m_EditingCancelled = true;
                    OnFinishEditingTitle();
                    break;
                case KeyCode.Return:
                    OnFinishEditingTitle();
                    break;
                default:
                    break;
            }
        }
        
        private void OnFinishEditingTitle()
        {
            // Show the label and hide the editor
            m_TitleLabel.visible = true;
            m_TitleEditor.style.display = DisplayStyle.None;

            if (!m_EditingCancelled)
            {
                string oldName = m_TitleLabel.text;
                string newName = m_TitleEditor.value;
                
                m_TitleLabel.text = newName;
                OnRenamed(oldName, newName);
            }
                
            m_EditingCancelled = false;
        }

        public virtual void OnRenamed(string oldName, string newName)
        {
            target.title = newName;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                if (HitTest(evt.localMousePosition))
                {
                    EditTitle();
                }
            }
        }
        
        private void EditTitle()
        {
            m_TitleLabel.visible = false;

            m_TitleEditor.SetValueWithoutNotify(target.title);
            m_TitleEditor.style.display = DisplayStyle.Flex;
            m_TitleEditor.Q(TextField.textInputUssName).Focus();
        }
        
        public override bool HitTest(Vector2 localPoint)
        {
            Vector2 mappedPoint = this.ChangeCoordinatesTo(m_TitleContainer, localPoint);
            return m_TitleContainer.ContainsPoint(mappedPoint);
        }
        
        private void MoveElements(Vector2 delta)
        {
            Debug.Log("move delta: " + delta);
            foreach (GraphElement element in containedNodes)
            {
                Rect r = element.GetPosition();

                r.position += delta;
                element.SetPosition(r);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            Vector2 delta = newPos.position - GetPosition().position;
            // m_Position = newPos.position;
            MoveElements(delta);

            base.SetPosition(newPos);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Debug.Log("geo change");

            // OnResize listening is bit of a workaround because we don't get resize
            // events on the GraphView. But I need to monitor for it for dirtying. 
            if (evt.newRect.width != evt.oldRect.width || evt.newRect.height != evt.oldRect.height)
            {
                OnResize();
            }
        }
        
        public void OnDirty()
        {
            Debug.Log("dirtied");
        }

        public void OnUpdate()
        {
            Debug.Log("updated");

            UpdateContained();
        }

        public void OnResize()
        {
            onResize?.Invoke(this);
        }

        public bool OverlapsElement(GraphElement element)
        {
            return worldBound.Overlaps(element.worldBound);
        }
        
        /// <summary>
        /// Scour the graph for new elements to add to the comment 
        /// after we've moved the comment or resized the bounds
        /// </summary>
        public void UpdateContained()
        {
            // Drop all nodes that are outside the bounds after a resize. 
            // TODO: This code is crap. 
            var removed = new List<NodeView>();
            containedNodes.ForEach((node) =>
            {
                if (!OverlapsElement(node))
                {
                    removed.Add(node);
                }
            });
            
            foreach (var node in removed)
            {
                node.comment = null;
                containedNodes.Remove(node);
            }
            
            // TODO: Optimal version, since this'll be slow af on large graphs
            GraphView gv = GetFirstAncestorOfType<GraphView>();

            gv.nodes.ForEach((node) =>
            {
                var nv = node as NodeView;
                if (nv != null && OverlapsElement(nv) && !containedNodes.Contains(nv))
                {
                    AddElement(nv);
                }
            });
        }

        public void RemoveElement(NodeView node)
        {
            if (node.comment == this)
            {
                node.comment = null;
            }

            containedNodes.Remove(node);
        }

        public void AddElement(NodeView node)
        {
            node.comment = this;
            containedNodes.Add(node);
        }
    }
}
