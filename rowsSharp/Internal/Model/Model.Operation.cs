namespace rowsSharp.Model;

internal enum OperationType
{
    Inline,
    Insert,
    Remove
}

internal class Operation
{
    internal OperationType OperationType { get; set; }
    internal int At { get; set; }
    internal Record OldRow { get; set; } = new();
    internal bool Parity { get; set; }

    internal static Operation DeepCopy(Operation copyFrom)
    {
        return new(copyFrom.OperationType, copyFrom.At, copyFrom.OldRow, copyFrom.Parity);
    }

    internal Operation(OperationType operationType, int at, Record oldRow, bool parity)
    {
        OperationType = operationType;
        At = at;
        OldRow = oldRow;
        Parity = parity;
    }
}
