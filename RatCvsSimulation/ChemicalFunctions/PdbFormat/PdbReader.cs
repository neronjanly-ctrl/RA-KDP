using System;
using System.IO;

namespace ChemicalFunctions.PdbFormat
{
    public class PdbReader : IDisposable
    {
        private readonly TextReader _reader = null;

        public PdbReader(TextReader reader)
        {
            _reader = reader;
        }

        public PdbReader(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        public PdbReader(string path)
        {
            _reader = new StreamReader(path);
        }

        public Record ReadRecord()
        {
            string line = _reader.ReadLine();
            if (line == "END")
                return EndRecord.CreateFromRow(line);

            if (line == null || line.Length > 80)
                return null;

            string tag = line.Substring(0, 6);
            if (!tag.TryParseRecordType(out var recordType))
            {
                Console.WriteLine($"Failed: unknown record type {tag}");
                return null;
            }

            try
            {
                switch (recordType)
                {
                    // Primary Structure Section
                    case RecordType.DbRef: return DbRefRecord.CreateFromRow(line);
                    case RecordType.DbRef1: return DbRef1Record.CreateFromRow(line);
                    case RecordType.DbRef2: return DbRef2Record.CreateFromRow(line);
                    case RecordType.SeqAdv: return SeqAdvRecord.CreateFromRow(line);
                    case RecordType.SeqRes: return SeqResRecord.CreateFromRow(line);
                    case RecordType.ModRes: return ModResRecord.CreateFromRow(line);

                    // Coordinate Section
                    case RecordType.Model: return ModelRecord.CreateFromRow(line);
                    case RecordType.Atom: return AtomRecord.CreateFromRow(line);
                    case RecordType.Anisou: return AnisouRecord.CreateFromRow(line);
                    case RecordType.Terminated: return TerminatedRecord.CreateFromRow(line);
                    case RecordType.HeteroAtom: return HeteroAtomRecord.CreateFromRow(line);
                    case RecordType.EndModel: return EndModelRecord.CreateFromRow(line);

                    // Others
                    default: return UnknownRecord.CreateFromRow(line);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Close()
        {
            _reader?.Close();
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
