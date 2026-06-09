namespace DeflexPro.Model;

public interface IFwdFileReader
{
    string FormatName { get; }
    bool CanRead(string fileName);
    Fwd Read(string fileName);
}
