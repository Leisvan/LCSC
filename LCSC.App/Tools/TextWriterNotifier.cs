using System;
using System.IO;
using System.Text;

namespace LCSC.App.Tools;

public enum TextWriteType
{
    Write,
    WriteLine,
}

public class TextWriterEventArgs(string? text, TextWriteType writeType) : EventArgs
{
    public string? Text { get; } = text;

    public TextWriteType WriteType { get; } = writeType;
}

public partial class TextWriterNotifier : TextWriter
{
    public event EventHandler<TextWriterEventArgs>? TextWritten;

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        TextWritten?.Invoke(this, new TextWriterEventArgs(value, TextWriteType.Write));
    }

    public override void WriteLine(string? value)
    {
        TextWritten?.Invoke(this, new TextWriterEventArgs(value, TextWriteType.WriteLine));
    }
}