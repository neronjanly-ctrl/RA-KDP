namespace ChemicalFunctions.PdbFormat
{
    public abstract class AbstractChainedRecord : Record
    {
        public char ChainId { get; set; }
    }
}
