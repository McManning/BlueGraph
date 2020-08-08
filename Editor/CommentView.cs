
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BlueGraph.Editor
{
    public class CommentView : GraphElement, ICanDirty
    {
        public Comment target;

        CommentTheme m_Theme;
        VisualElement m_TitleContainer;
        TextField m_TitleEditor;
        Label m_TitleLabel;
        bool m_EditingCancelled;

        public CommentView(Comment comment)
        {
            target = comment;
            SetPosition(comment.region);
            
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/CommentView"));
            
            m_TitleContainer = new VisualElement();
            m_TitleContainer.AddToClassList("titleContainer");
            
            m_TitleEditor = new TextField();
            
            var input = m_TitleEditor.Q(TextField.textInputUssName);
            input.RegisterCallback<KeyDownEvent>(OnTitleKeyDown);
            input.RegisterCallback<FocusOutEvent>(e => { OnFinishEditingTitle(); });
            
            m_TitleContainer.Add(m_TitleEditor);
            
            m_TitleLabel = new Label();
            m_TitleLabel.text = comment.text;
            
            m_TitleContainer.Add(m_TitleLabel);

            m_TitleEditor.style.display = DisplayStyle.None;

            Add(m_TitleContainer);

            ClearClassList();
            AddToClassList("commentView");
            
            capabilities |= Capabilities.Selectable | Capabilities.Movable | 
                            Capabilities.Deletable | Capabilities.Resizable;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            
            SetTheme(target.theme);
        }
        
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is CommentView)
            {
                // Add options to change theme
                foreach (var theme in (CommentTheme[])Enum.GetValues(typeof(CommentTheme)))
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
        
        /// <summary>
        /// Change the color theme used on the canvas
        /// </summary>
        public void SetTheme(CommentTheme theme)
        {
            RemoveFromClassList("theme-" + m_Theme);
            AddToClassList("theme-" + theme);
            m_Theme = theme;
            target.theme = theme;
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
        
        private void EditTitle()
        {
            m_TitleLabel.visible = false;

            m_TitleEditor.SetValueWithoutNotify(target.text);
            m_TitleEditor.style.display = DisplayStyle.Flex;
            m_TitleEditor.Q(TextField.textInputUssName).Focus();
        }
        
        public virtual void OnRenamed(string oldName, string newName)
        {
            target.text = newName;
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
        
        /// <summary>
        /// Override HitTest to only trigger when they click the title
        /// </summary>
        public override bool HitTest(Vector2 localPoint)
        {
            Vector2 mappedPoint = this.ChangeCoordinatesTo(m_TitleContainer, localPoint);
            return m_TitleContainer.ContainsPoint(mappedPoint);
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            target.region = newPos;
        }

        public void OnDirty()
        {

        }

        public void OnUpdate()
        {

        }
    }
}
