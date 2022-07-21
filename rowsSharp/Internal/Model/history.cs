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
        internal bool Parity { get; set; }

        static internal Operation DeepCopy(Operation copyFrom)
        {
            return new Operation()
            {
                OperationEnum = copyFrom.OperationEnum,
                At = copyFrom.At,
                OldRow = copyFrom.OldRow,
                Parity = copyFrom.Parity
            };
        }
    }
}
