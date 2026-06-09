using System.IO;

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
            ?? throw new InvalidDataException("Nem támogatott FWD mérési fájlformátum.");

        var measurement = reader.Read(fileName);
        measurement.SourceFileName = Path.GetFullPath(fileName);
        measurement.FormatName = reader.FormatName;
        return measurement;
    }
}
