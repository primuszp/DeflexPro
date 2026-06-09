using System.Globalization;
using System.IO;

namespace DeflexPro.Model;

public sealed class PrimaxFileReader : IFwdFileReader
{
    public string FormatName => "Primax";

    public bool CanRead(string fileName)
    {
        using var reader = File.OpenText(fileName);
        for (var index = 0; index < 30 && reader.ReadLine() is { } line; index++)
        {
            if (line.StartsWith("(c) ROAD SYSTEM", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public Fwd Read(string fileName)
    {
        var result = new Primax();
        var date = DateTime.MinValue;
        var chainage = double.NaN;
        var time = TimeSpan.Zero;
        var expectsMeasurement = false;

        foreach (var rawLine in File.ReadLines(fileName))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("Date [dd/mm/yy]:", StringComparison.Ordinal))
                date = ParseDate(ValueAfterTab(line), "dd/MM/yyyy");
            else if (line.StartsWith("Load plate radius [mm].", StringComparison.Ordinal))
                result.PlateRadius = ParseDouble(ValueAfterTab(line));
            else if (line.StartsWith("Radial offset [cm].....", StringComparison.Ordinal))
                SetSensors(result, line);
            else if (line.StartsWith("Chainage [m]...........", StringComparison.Ordinal))
                chainage = ParseDouble(ValueAfterTab(line));
            else if (line.StartsWith("Sequence:", StringComparison.Ordinal))
                time = ParseSequenceTime(line);
            else if (line.StartsWith("Drop\tD(1)", StringComparison.Ordinal))
                expectsMeasurement = true;
            else if (expectsMeasurement && char.IsDigit(line.FirstOrDefault()))
            {
                result.Drops.Add(ParseMeasurement(line, result.Sensors, chainage, date, time));
                expectsMeasurement = false;
            }
        }

        if (result.Sensors.Count == 0 || result.Drops.Count == 0)
            throw new InvalidDataException("A Primax fájl nem tartalmaz feldolgozható mérési adatot.");

        return result;
    }

    private static void SetSensors(Fwd result, string line)
    {
        result.Sensors.Clear();
        var columns = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        var offsets = columns.Skip(1);
        var index = 0;
        foreach (var offset in offsets)
            result.Sensors.Add(new Sensor(index++, ParseDouble(offset) * 10d, 0));
    }

    private static Drop ParseMeasurement(
        string line,
        IReadOnlyList<Sensor> sensors,
        double chainage,
        DateTime date,
        TimeSpan time)
    {
        var values = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < sensors.Count + 6)
            throw new InvalidDataException("Hiányos Primax mérési adatsor.");

        var drop = new Drop
        {
            ImpNumber = int.Parse(values[0], CultureInfo.InvariantCulture),
            Distance = chainage,
            PeakForce = ParseDouble(values[sensors.Count + 2]) * 100d,
            AirTemperature = ParseDouble(values[sensors.Count + 3]),
            AsphaltTemperature = ParseDouble(values[sensors.Count + 4]),
            DateTime = date.Date + time
        };

        for (var index = 0; index < sensors.Count; index++)
            drop.Deflections.Add(new Deflection(sensors[index], ParseDouble(values[index + 1])));

        return drop;
    }

    private static TimeSpan ParseSequenceTime(string line)
    {
        var marker = "Time:";
        var index = line.IndexOf(marker, StringComparison.Ordinal);
        return index < 0
            ? TimeSpan.Zero
            : TimeSpan.Parse(line[(index + marker.Length)..].Trim(), CultureInfo.InvariantCulture);
    }

    private static string ValueAfterTab(string line) =>
        line.Split('\t', StringSplitOptions.RemoveEmptyEntries).Last().Trim();

    private static DateTime ParseDate(string value, string format) =>
        DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);

    private static double ParseDouble(string value) =>
        double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
}
