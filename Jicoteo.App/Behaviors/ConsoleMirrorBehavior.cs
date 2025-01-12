﻿using CommunityToolkit.WinUI.Behaviors;
using LCSC.Manager.Tools;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace LCSC.App.Behaviors
{
    public class ConsoleMirrorBehavior : BehaviorBase<RichTextBlock>
    {
        private SolidColorBrush? _colorBrush;
        private ConsoleColor? _lastConsoleColor;
        private TextWriterNotifier _notifier;

        public ConsoleMirrorBehavior()
        {
            _notifier = new TextWriterNotifier();
            _notifier.TextWritten += TextWritten;
            Console.SetOut(_notifier);
        }

        private static Color ConvertConsoleColorToColor(ConsoleColor consoleColor)
        {
            return consoleColor switch
            {
                ConsoleColor.Black => Colors.Black,
                ConsoleColor.White => Colors.White,
                ConsoleColor.Yellow => Colors.Yellow,
                ConsoleColor.Red => Colors.Red,
                ConsoleColor.DarkBlue => Colors.DarkBlue,
                ConsoleColor.DarkGreen => Colors.DarkGreen,
                ConsoleColor.DarkCyan => Colors.DarkCyan,
                ConsoleColor.DarkRed => Colors.DarkRed,
                ConsoleColor.DarkMagenta => Colors.DarkMagenta,
                ConsoleColor.DarkYellow => Colors.Olive,
                ConsoleColor.Gray => Colors.Gray,
                ConsoleColor.DarkGray => Colors.DarkGray,
                ConsoleColor.Blue => Colors.Blue,
                ConsoleColor.Green => Colors.Green,
                ConsoleColor.Cyan => Colors.Cyan,
                ConsoleColor.Magenta => Colors.Magenta,
                _ => Colors.White,
            };
        }

        private void SetColor()
        {
            var ccolor = Console.ForegroundColor;
            if (ccolor != _lastConsoleColor)
            {
                _lastConsoleColor = ccolor;
                _colorBrush = new SolidColorBrush(ConvertConsoleColorToColor(ccolor));
            }
        }

        private void TextWritten(object? sender, TextWriterEventArgs e)
        {
            if (AssociatedObject != null)
            {
                SetColor();
                var p = new Paragraph();
                var r = new Run { Text = e.Text };
                p.Inlines.Add(r);
                AssociatedObject.Blocks.Add(p);
            }
        }
    }
}