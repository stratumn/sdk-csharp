namespace Stratumn.Sdk.Model.Trace
{
    using System.Collections.Generic;

    /// <summary>
    /// The various action types.
    /// </summary>
    public sealed class TraceActionType
    {
        public static readonly TraceActionType ATTESTATION = new TraceActionType("ATTESTATION", InnerEnum.ATTESTATION, "_ATTESTATION_");
        public static readonly TraceActionType PUSH_OWNERSHIP = new TraceActionType("PUSH_OWNERSHIP", InnerEnum.PUSH_OWNERSHIP, "_PUSH_OWNERSHIP_");
        public static readonly TraceActionType PULL_OWNERSHIP = new TraceActionType("PULL_OWNERSHIP", InnerEnum.PULL_OWNERSHIP, "_PULL_OWNERSHIP_");
        public static readonly TraceActionType ACCEPT_TRANSFER = new TraceActionType("ACCEPT_TRANSFER", InnerEnum.ACCEPT_TRANSFER, "_ACCEPT_TRANSFER_");
        public static readonly TraceActionType CANCEL_TRANSFER = new TraceActionType("CANCEL_TRANSFER", InnerEnum.CANCEL_TRANSFER, "_CANCEL_TRANSFER_");
        public static readonly TraceActionType REJECT_TRANSFER = new TraceActionType("REJECT_TRANSFER", InnerEnum.REJECT_TRANSFER, "_REJECT_TRANSFER_");

        private static readonly IList<TraceActionType> valueList = new List<TraceActionType>();

        static TraceActionType()
        {
            valueList.Add(ATTESTATION);
            valueList.Add(PUSH_OWNERSHIP);
            valueList.Add(PULL_OWNERSHIP);
            valueList.Add(ACCEPT_TRANSFER);
            valueList.Add(CANCEL_TRANSFER);
            valueList.Add(REJECT_TRANSFER);
        }

        public enum InnerEnum
        {
            ATTESTATION,
            PUSH_OWNERSHIP,
            PULL_OWNERSHIP,
            ACCEPT_TRANSFER,
            CANCEL_TRANSFER,
            REJECT_TRANSFER
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private string value;

        private TraceActionType(string name, InnerEnum innerEnum, string value)
        {
            this.value = value;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        public override string ToString()
        {
            return value;

        }

        public static IList<TraceActionType> Values()
        {
            return valueList;
        }

        public int Ordinal()
        {
            return ordinalValue;
        }

        public static TraceActionType ValueOf(string name)
        {
            foreach (TraceActionType enumInstance in TraceActionType.valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }
}
