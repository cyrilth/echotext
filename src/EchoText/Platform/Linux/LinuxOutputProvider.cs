using System;
using System.Threading.Tasks;
using EchoText.Platform.Interfaces;
using SharpHook;
using SharpHook.Native;

namespace EchoText.Platform.Linux;

/// <summary>
/// Linux implementation of text output (auto-typing) using SharpHook
/// Works on X11. On Wayland, functionality may be limited by security restrictions.
/// </summary>
public class LinuxOutputProvider : IPlatformOutput
{
    private readonly EventSimulator _simulator;

    public LinuxOutputProvider()
    {
        _simulator = new EventSimulator();
    }

    public async Task TypeTextAsync(string text, int delayMs = 10)
    {
        if (string.IsNullOrEmpty(text))
            return;

        foreach (char c in text)
        {
            // Simulate typing each character
            SimulateChar(c);

            // Add delay between keystrokes if specified
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }
        }
    }

    private void SimulateChar(char c)
    {
        // Handle special characters
        switch (c)
        {
            case '\n':
                SimulateKey(KeyCode.VcEnter);
                break;
            case '\r':
                // Ignore carriage return (handled by \n)
                break;
            case '\t':
                SimulateKey(KeyCode.VcTab);
                break;
            case ' ':
                SimulateKey(KeyCode.VcSpace);
                break;
            default:
                SimulateTextChar(c);
                break;
        }
    }

    private void SimulateTextChar(char c)
    {
        // Check if character requires shift key
        bool needsShift = char.IsUpper(c) || IsShiftedSymbol(c);

        var keyCode = MapCharToKeyCode(c);

        if (keyCode == KeyCode.VcUndefined)
        {
            // If we can't map the character, use Unicode text input
            SimulateUnicodeChar(c);
            return;
        }

        if (needsShift)
        {
            // Press shift
            _simulator.SimulateKeyPress(KeyCode.VcLeftShift);
        }

        // Press and release the key
        _simulator.SimulateKeyPress(keyCode);
        _simulator.SimulateKeyRelease(keyCode);

        if (needsShift)
        {
            // Release shift
            _simulator.SimulateKeyRelease(KeyCode.VcLeftShift);
        }
    }

    private void SimulateKey(KeyCode keyCode)
    {
        _simulator.SimulateKeyPress(keyCode);
        _simulator.SimulateKeyRelease(keyCode);
    }

    private void SimulateUnicodeChar(char c)
    {
        // For characters we can't map, simulate using Unicode text input
        // This is a fallback for special characters
        _simulator.SimulateTextEntry(c.ToString());
    }

    private static bool IsShiftedSymbol(char c)
    {
        return c switch
        {
            '!' or '@' or '#' or '$' or '%' or '^' or '&' or '*' or '(' or ')' => true,
            '_' or '+' or '{' or '}' or '|' or ':' or '"' or '<' or '>' or '?' => true,
            '~' => true,
            _ => false
        };
    }

    private static KeyCode MapCharToKeyCode(char c)
    {
        // Convert to lowercase for mapping
        char lower = char.ToLower(c);

        return lower switch
        {
            // Letters
            'a' => KeyCode.VcA,
            'b' => KeyCode.VcB,
            'c' => KeyCode.VcC,
            'd' => KeyCode.VcD,
            'e' => KeyCode.VcE,
            'f' => KeyCode.VcF,
            'g' => KeyCode.VcG,
            'h' => KeyCode.VcH,
            'i' => KeyCode.VcI,
            'j' => KeyCode.VcJ,
            'k' => KeyCode.VcK,
            'l' => KeyCode.VcL,
            'm' => KeyCode.VcM,
            'n' => KeyCode.VcN,
            'o' => KeyCode.VcO,
            'p' => KeyCode.VcP,
            'q' => KeyCode.VcQ,
            'r' => KeyCode.VcR,
            's' => KeyCode.VcS,
            't' => KeyCode.VcT,
            'u' => KeyCode.VcU,
            'v' => KeyCode.VcV,
            'w' => KeyCode.VcW,
            'x' => KeyCode.VcX,
            'y' => KeyCode.VcY,
            'z' => KeyCode.VcZ,

            // Numbers
            '0' => KeyCode.Vc0,
            '1' => KeyCode.Vc1,
            '2' => KeyCode.Vc2,
            '3' => KeyCode.Vc3,
            '4' => KeyCode.Vc4,
            '5' => KeyCode.Vc5,
            '6' => KeyCode.Vc6,
            '7' => KeyCode.Vc7,
            '8' => KeyCode.Vc8,
            '9' => KeyCode.Vc9,

            // Symbols (unshifted)
            '`' => KeyCode.VcBackQuote,
            '-' => KeyCode.VcMinus,
            '=' => KeyCode.VcEquals,
            '[' => KeyCode.VcOpenBracket,
            ']' => KeyCode.VcCloseBracket,
            '\\' => KeyCode.VcBackslash,
            ';' => KeyCode.VcSemicolon,
            '\'' => KeyCode.VcQuote,
            ',' => KeyCode.VcComma,
            '.' => KeyCode.VcPeriod,
            '/' => KeyCode.VcSlash,

            // Symbols (shifted)
            '~' => KeyCode.VcBackQuote,
            '!' => KeyCode.Vc1,
            '@' => KeyCode.Vc2,
            '#' => KeyCode.Vc3,
            '$' => KeyCode.Vc4,
            '%' => KeyCode.Vc5,
            '^' => KeyCode.Vc6,
            '&' => KeyCode.Vc7,
            '*' => KeyCode.Vc8,
            '(' => KeyCode.Vc9,
            ')' => KeyCode.Vc0,
            '_' => KeyCode.VcMinus,
            '+' => KeyCode.VcEquals,
            '{' => KeyCode.VcOpenBracket,
            '}' => KeyCode.VcCloseBracket,
            '|' => KeyCode.VcBackslash,
            ':' => KeyCode.VcSemicolon,
            '"' => KeyCode.VcQuote,
            '<' => KeyCode.VcComma,
            '>' => KeyCode.VcPeriod,
            '?' => KeyCode.VcSlash,

            _ => KeyCode.VcUndefined
        };
    }
}
