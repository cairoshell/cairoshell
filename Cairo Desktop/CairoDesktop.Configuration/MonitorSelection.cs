using System.Drawing;
using System.Globalization;

namespace CairoDesktop.Configuration
{
    public abstract class MonitorPreference
    {
        protected MonitorPreference() { }

        public static MonitorPreference Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (value == "Primary") return PrimaryMonitorPreference.Instance;

            if (value.StartsWith("Pos=", System.StringComparison.Ordinal))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 2)
                {
                    if (TryParseInt(parts[0].Substring(4), out int x) && TryParseInt(parts[1], out int y))
                    {
                        return new MonitorFromCoordinatesPreference(new Point(x, y));
                    }
                }
                return null;
            }

            return null;
        }

        static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
    
    public sealed class PrimaryMonitorPreference : MonitorPreference
    {
        private PrimaryMonitorPreference() { }
        internal static readonly PrimaryMonitorPreference Instance = new PrimaryMonitorPreference();
        public override string ToString() => "Primary";
    }
    
    public sealed class MonitorFromCoordinatesPreference : MonitorPreference
    {
        public Point Coordinates { get; set; }
        public MonitorFromCoordinatesPreference(Point coordinates)
        {
            this.Coordinates = coordinates;
        }
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Pos={0},{1}", Coordinates.X, Coordinates.Y);
    }
}
