using System;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>Validates names, device letters and paths before they are written into ZPL, so they cannot inject additional commands.</summary>
    internal static class ZplIdentifier
    {
        /// <summary>The ZPL storage devices: <c>R</c> (DRAM), <c>E</c> (flash), <c>B</c> (memory card), <c>A</c> (auto) and <c>Z</c> (firmware).</summary>
        private const string Devices = "REBAZ";

        /// <summary>Checks a storage device letter.</summary>
        public static char Device(char device, string paramName)
        {
            if (Devices.IndexOf(device) < 0)
            {
                throw new ArgumentException("The storage device must be one of R, E, B, A or Z.", paramName);
            }

            return device;
        }

        /// <summary>The devices a download (<c>~DG</c>, <c>~DY</c>) can write to: <c>R</c>, <c>E</c>, <c>B</c> and <c>A</c>; <c>Z</c> is read-only firmware.</summary>
        private const string DownloadDevices = "REBA";

        /// <summary>Checks the storage device of a download; <c>Z</c> is not writable.</summary>
        public static char DownloadDevice(char device, string paramName)
        {
            if (DownloadDevices.IndexOf(device) < 0)
            {
                throw new ArgumentException("The storage device of a download must be one of R, E, B or A.", paramName);
            }

            return device;
        }

        /// <summary>Checks a device given as string (a single letter without colon).</summary>
        public static string Device(string device, string paramName)
        {
            ArgumentNullException.ThrowIfNull(device, paramName);
            if (device.Length != 1)
            {
                throw new ArgumentException("The storage device must be a single letter (R, E, B, A or Z) without colon.", paramName);
            }

            Device(device[0], paramName);
            return device;
        }

        /// <summary>Checks a letter or digit identifier (ASCII only).</summary>
        public static char LetterOrDigit(char value, string paramName)
        {
            if (!char.IsAsciiLetterOrDigit(value))
            {
                throw new ArgumentException("Only an ASCII letter or digit (A-Z, 0-9) is allowed.", paramName);
            }

            return value;
        }

        /// <summary>Checks a font identifier of <c>^A</c>: a single ASCII letter or digit.</summary>
        public static string FontName(string value, string paramName)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            if (value.Length != 1)
            {
                throw new ArgumentException("The font name must be a single ASCII letter or digit.", paramName);
            }

            LetterOrDigit(value[0], paramName);
            return value;
        }

        /// <summary>Checks a name: no <c>^ ~ , :</c>, no whitespace or control characters and, unless allowed, no dot.</summary>
        public static string Name(string value, string paramName, bool allowDot, int maxLength = int.MaxValue, bool allowEmpty = false)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            if (value.Length == 0)
            {
                if (allowEmpty)
                {
                    return value;
                }

                throw new ArgumentException("The value must not be empty.", paramName);
            }

            if (value.Length > maxLength)
            {
                throw new ArgumentException($"Maximum length of {maxLength} characters exceeded", paramName);
            }

            foreach (char c in value)
            {
                if (c == '^' || c == '~' || c == ',' || c == ':' || char.IsWhiteSpace(c) || char.IsControl(c) || (c == '.' && !allowDot))
                {
                    throw new ArgumentException("The value contains a character that is not allowed in a ZPL name (^ ~ , : . whitespace or control characters).", paramName);
                }
            }

            return value;
        }

        /// <summary>The name the printer uses when the object name of a download or recall is omitted.</summary>
        public const string UnknownName = "UNKNOWN";

        /// <summary>Checks an object name like <see cref="Name"/>; an empty name becomes <c>UNKNOWN</c>, the name the printer uses when it is omitted.</summary>
        public static string ObjectName(string value, string paramName, bool allowDot, int maxLength = int.MaxValue)
        {
            return Name(value, paramName, allowDot, maxLength, allowEmpty: true).Length == 0 ? UnknownName : value;
        }

        /// <summary>Checks a graphic name with an optional extension (<c>LOGO</c> or <c>LOGO.GRF</c>): 1 to 8 characters before the extension; an empty name (also <c>.GRF</c>) becomes <c>UNKNOWN</c>.</summary>
        public static string GraphicName(string value, string paramName)
        {
            string name = ObjectName(value, paramName, allowDot: true);
            if (name[0] == '.')
            {
                // Only the extension is given (".GRF"): the printer uses UNKNOWN as name
                name = UnknownName + name;
            }

            int dot = name.IndexOf('.');
            int baseLength = dot < 0 ? name.Length : dot;
            if (baseLength > 8 || name.EndsWith('.') || name.IndexOf('.', dot + 1) >= 0)
            {
                throw new ArgumentException("The graphic name must have 1 to 8 characters, optionally followed by one extension (for example LOGO or LOGO.GRF).", paramName);
            }

            return name;
        }

        /// <summary>Checks a file name with optional device prefix (<c>R:NAME.EXT</c>). An empty name is allowed; the printer then uses <c>UNKNOWN</c>.</summary>
        public static string Path(string value, string paramName)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            int colon = value.IndexOf(':');
            if (colon >= 0)
            {
                if (colon != 1)
                {
                    throw new ArgumentException("The device prefix must be a single letter followed by a colon (for example R:NAME.ZPL).", paramName);
                }

                Device(value[0], paramName);
                Name(value.Substring(2), paramName, allowDot: true, allowEmpty: true);
                return value;
            }

            return Name(value, paramName, allowDot: true, allowEmpty: true);
        }
    }
}
