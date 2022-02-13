﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace YWR.Tools
{
    public partial class DialogSystem : MonoBehaviour, IDialogSystem
    {
        [Tooltip("The text which contains the author content.")] [SerializeField]
        private TextMeshProUGUI authorText;

        [Tooltip("Parent transform to hold all dialog buttons.")] [SerializeField]
        private Transform buttonsAnchor;

        [Header("Set by Editor")] [Tooltip("All the window content.")] [SerializeField]
        private GameObject content;

        [Tooltip("Parameters to configure the writing.")] [SerializeField]
        private Parameters parameters;

        [Tooltip("The text which contains the written sentence.")] [SerializeField]
        private TextMeshProUGUI sentenceText;

        // -----------------------------------------------------------------------------------------

        private List<DialogButton> CurrentButtons { get; } = new List<DialogButton>();
        private DialogAnimation Animation { get; set; }
        private DialogWriting Writing { get; set; }
        private DialogSequence Sequence { get; set; }

        protected void Awake()
        {
            Animation = new DialogAnimation(this);
            Writing = new DialogWriting(this, sentenceText, authorText);
            Sequence = new DialogSequence(this);
            OnShow += Writing.StartWriting;
            OnShow += () => CreateButtons(Sequence.GetCurrent());
            Hide();
        }

        public int Speed => parameters.speed;
        public bool IsOpened { get; private set; }
        public Action OnShow { get; set; } = () => { };
        public Action OnHide { get; set; } = () => { };
        public Action OnFinishSequence { get; set; } = () => { };
        public MonoBehaviour Monobehavior => this;

        //-----------------------------------------------------------------------------------------

        #region Write and Clear

        public void Write(TextSequence textSequence)
        {
            Sequence.SetSequence(textSequence);
            TextPiece current = Sequence.GetCurrent();
            if (current == null)
            {
                return;
            }

            string author = current.Author;
            string text = current.Text;
            Write(text, author);
        }

        private void Write(string text, string author)
        {
            Writing.Write(text, author);
        }


        #region Next

        public void Next()
        {
            if (!IsOpened)
            {
                return;
            }

            if (Sequence == null)
            {
                return;
            }

            if (Sequence.IsLast)
            {
                OnFinishSequence?.Invoke();
                Hide();
                return;
            }

            TextPiece current = Sequence?.GetCurrent();
            if (current == null)
            {
                return;
            }

            Clear();
            WriteNext();
        }

        private void WriteNext()
        {
            TextPiece next = Sequence.GetNext();
            if (next == null)
            {
                return;
            }

            string author = next.Author;
            string text = next.Text;
            CreateButtons(next);
            Write(text, author);
        }

        #endregion

        private void CreateButtons(TextPiece next)
        {
            foreach (TextButton piece in next.Buttons)
            {
                DialogButton btn = piece.CreateButton(buttonsAnchor, this);
                CurrentButtons.Add(btn);
            }
        }

        [Button]
        public void Clear()
        {
            ClearButtons();
            Writing.Clear();
        }

        private void ClearButtons()
        {
            for (int i = 0; i < CurrentButtons.Count; i++)
            {
                Destroy(CurrentButtons[i].gameObject);
            }

            CurrentButtons.Clear();
        }

        #endregion

        //-----------------------------------------------------------------------------------------

        #region Show and Hide

        [Button]
        public void Show()
        {
            Animation.Show();
            IsOpened = true;
        }

        [Button]
        public void Hide()
        {
            Animation.Hide();
            Sequence.Reset();
            IsOpened = false;
            Clear();
        }

        #endregion

        //-----------------------------------------------------------------------------------------

        #region Activate and Deactivate

        [Button]
        public void Activate()
        {
            content.SetActive(true);
        }

        [Button]
        public void Deactivate()
        {
            content.SetActive(false);
            Hide();
        }

        #endregion

        //-----------------------------------------------------------------------------------------
    }
}