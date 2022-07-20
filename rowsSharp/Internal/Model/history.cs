namespace rowsSharp.Model
{
    public enum OperationEnum
    {
        Inline,
        Insert,
        Remove
    }

    public class Operation
    {
        internal OperationEnum OperationEnum { get; set; }
        internal int At { get; set; }
        internal CsvRecord OldRow { get; set; } = new();
    }
}
