using CommunityToolkit.WinUI.Behaviors;
using LCSC.Manager.Tools;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;

namespace LCSC.App.Behaviors
{
    public class ConsoleMirrorBehavior : BehaviorBase<RichTextBlock>
    {
        private TextWriterNotifier _notifier;

        public ConsoleMirrorBehavior()
        {
            _notifier = new TextWriterNotifier();
            _notifier.TextWritten += TextWritten;
            Console.SetOut(_notifier);
        }

        private void TextWritten(object? sender, TextWriterEventArgs e)
        {
            if (AssociatedObject != null)
            {
                var p = new Paragraph();
                var r = new Run { Text = e.Text };
                p.Inlines.Add(r);
                AssociatedObject.Blocks.Add(p);
            }
        }
    }
}