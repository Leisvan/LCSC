using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Discord.Helpers;

public class StringLengthCapTool(int length, bool invertFormat = false)
{
    private const int MaxLength = 12;
    private const string ThreeDots = "…";
    private readonly bool invertedFormat = invertFormat;

    public StringLengthCapTool(IEnumerable<string> sample, bool invertFormat = false)
        : this(CalculateLength(sample, MaxLength), invertFormat)
    {
    }

    public int Length { get; private set; } = length;

    #region Static

    private static readonly StringLengthCapTool _default;
    private static readonly StringLengthCapTool _i2spaces;
    private static readonly StringLengthCapTool _i3spaces;
    private static readonly StringLengthCapTool _i4spaces;
    private static readonly StringLengthCapTool _i5spaces;

    static StringLengthCapTool()
    {
        _i2spaces = new StringLengthCapTool(2, true);
        _i3spaces = new StringLengthCapTool(3, true);
        _i4spaces = new StringLengthCapTool(4, true);
        _i5spaces = new StringLengthCapTool(5, true);
        _default = new StringLengthCapTool(10, false);
    }

    public static StringLengthCapTool Default => _default;

    public static StringLengthCapTool InvertedFiveSpaces => _i5spaces;

    public static StringLengthCapTool InvertedFourSpaces => _i4spaces;

    public static StringLengthCapTool InvertedThreeSpaces => _i3spaces;

    public static StringLengthCapTool InvertedTwoSpaces => _i2spaces;

    #endregion Static

    public string GetString(string? text)
    {
        if (text == null)
        {
            return string.Empty;
        }
        if (text.Length > Length)
        {
            string trimmed = text[..(Length - 1)];
            return string.Concat(trimmed, ThreeDots);
        }
        var blankSpaces = GetBlankSpaces(Length - text.Length);
        return invertedFormat ? string.Concat(blankSpaces, text) : string.Concat(text, blankSpaces);
    }

    public string GetString(object obj)
        => GetString(obj.ToString());

    private static int CalculateLength(IEnumerable<string> sample, int maxLength)
    {
        int sampleMax = sample.Select(x => x.Length).Max();
        return Math.Min(sampleMax, maxLength);
    }

    private static string GetBlankSpaces(int count)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            builder.Append(' ');
        }
        return builder.ToString();
    }
}