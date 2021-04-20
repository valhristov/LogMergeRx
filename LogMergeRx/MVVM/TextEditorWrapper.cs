using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LogMergeRx.MVVM
{
    /// <summary>
    /// https://stackoverflow.com/a/45627524/7156760
    /// </summary>
    public class TextEditorWrapper
    {
        private static readonly Type _textEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo _isReadOnlyProp = _textEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo _textViewProp = _textEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _registerMethod = _textEditorType.GetMethod("RegisterCommandHandlers", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);
        private static readonly Type _textContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo _textContainerTextViewProp = _textContainerType.GetProperty("TextView");

        private static readonly PropertyInfo _textContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
        {
            _registerMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
        }

        public static TextEditorWrapper CreateFor(TextBlock tb)
        {
            var textContainer = _textContainerProp.GetValue(tb);

            var editor = new TextEditorWrapper(textContainer, tb, false);
            _isReadOnlyProp.SetValue(editor._editor, true);
            _textViewProp.SetValue(editor._editor, _textContainerTextViewProp.GetValue(textContainer));

            return editor;
        }

        private readonly object _editor;

        public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
        {
            _editor = Activator.CreateInstance(_textEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new[] { textContainer, uiScope, isUndoEnabled }, null);
        }
    }
}
