using System.IO;
using DeflexPro.Localization;

namespace DeflexPro.Model;

public sealed class FwdFileReaderFactory
{
    private readonly IReadOnlyList<IFwdFileReader> readers;

    public FwdFileReaderFactory()
        : this(new IFwdFileReader[] { new KuabFileReader(), new PrimaxFileReader() })
    {
    }

    public FwdFileReaderFactory(IEnumerable<IFwdFileReader> readers)
    {
        this.readers = readers.ToArray();
    }

    public Fwd Read(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var reader = readers.FirstOrDefault(candidate => candidate.CanRead(fileName))
            ?? throw new InvalidDataException(Localizer.Get("UnsupportedFwdFormat", "Unsupported FWD measurement file format."));

        var measurement = reader.Read(fileName);
        measurement.SourceFileName = Path.GetFullPath(fileName);
        measurement.FormatName = reader.FormatName;
        return measurement;
    }
}
