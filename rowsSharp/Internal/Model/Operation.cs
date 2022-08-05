namespace rowsSharp.Model;

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
    internal Record OldRow { get; set; } = new();
    internal bool Parity { get; set; }

    internal static Operation DeepCopy(Operation copyFrom)
    {
        return new()
        {
            OperationEnum = copyFrom.OperationEnum,
            At = copyFrom.At,
            OldRow = copyFrom.OldRow,
            Parity = copyFrom.Parity
        };
    }
}
