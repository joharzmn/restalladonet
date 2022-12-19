using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace RESTAll.Data.Common
{
    internal static class ADP
    {
        private static Task<bool> s_trueTask;
        private static Task<bool> s_falseTask;
        internal const int DefaultConnectionTimeout = 15;
        internal const CompareOptions compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

        internal static Task<bool> TrueTask
        {
            get
            {
                if (ADP.s_trueTask == null)
                    ADP.s_trueTask = Task.FromResult<bool>(true);
                return ADP.s_trueTask;
            }
        }

        internal static Task<bool> FalseTask
        {
            get
            {
                if (ADP.s_falseTask == null)
                    ADP.s_falseTask = Task.FromResult<bool>(false);
                return ADP.s_falseTask;
            }
        }

        internal static ArgumentException Argument(string error) => new ArgumentException(error);

        internal static ArgumentException Argument(string error, Exception inner) => new ArgumentException(error, inner);

        internal static ArgumentException Argument(string error, string parameter) => new ArgumentException(error, parameter);

        internal static ArgumentNullException ArgumentNull(string parameter) => new ArgumentNullException(parameter);

        internal static InvalidOperationException InvalidOperation(string error) => new InvalidOperationException(error);

        internal static NotSupportedException NotSupported() => new NotSupportedException();

        internal static void CheckArgumentLength(string value, string parameterName)
        {
            ADP.CheckArgumentNull((object)value, parameterName);
            if (value.Length == 0)
                throw ADP.Argument(SR.Format(SR.ADP_EmptyString, (object)parameterName));
        }

        internal static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
                throw ADP.ArgumentNull(parameterName);
        }

        internal static ArgumentException ConnectionStringSyntax(int index) => ADP.Argument(SR.Format(SR.ADP_ConnectionStringSyntax, (object)index));

        internal static ArgumentException KeywordNotSupported(string keyword) => ADP.Argument(SR.Format(SR.ADP_KeywordNotSupported, (object)keyword));

        internal static ArgumentException InvalidKeyname(string parameterName) => ADP.Argument(SR.Format(SR.ADP_InvalidKey), parameterName);

        internal static ArgumentException InvalidValue(string parameterName) => ADP.Argument(SR.Format(SR.ADP_InvalidValue), parameterName);

        internal static ArgumentException ConvertFailed(
          Type fromType,
          Type toType,
          Exception innerException)
        {
            return ADP.Argument(SR.Format(SR.SqlConvert_ConvertFailed, (object)fromType.FullName, (object)toType.FullName), innerException);
        }

        internal static Exception InternalError(ADP.InternalErrorCode internalError) => (Exception)ADP.InvalidOperation(SR.Format(SR.ADP_InternalProviderError, (object)(int)internalError));

        internal static bool IsEmpty(string str) => string.IsNullOrEmpty(str);

        internal static IndexOutOfRangeException IndexOutOfRange(string error) => new IndexOutOfRangeException(error);

        internal static ArgumentOutOfRangeException InvalidSourceBufferIndex(
          int maxLen,
          long srcOffset,
          string parameterName)
        {
            return ADP.ArgumentOutOfRange(SR.Format(SR.ADP_InvalidSourceBufferIndex, (object)maxLen.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)srcOffset.ToString((IFormatProvider)CultureInfo.InvariantCulture)), parameterName);
        }

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(
          string message,
          string parameterName)
        {
            return new ArgumentOutOfRangeException(parameterName, message);
        }

        internal static Exception InvalidDataLength(long length) => (Exception)ADP.IndexOutOfRange(SR.Format(SR.SQL_InvalidDataLength, (object)length.ToString((IFormatProvider)CultureInfo.InvariantCulture)));

        internal static IndexOutOfRangeException InvalidBufferSizeOrIndex(
          int numBytes,
          int bufferIndex)
        {
            return ADP.IndexOutOfRange(SR.Format(SR.SQL_InvalidBufferSizeOrIndex, (object)numBytes.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)bufferIndex.ToString((IFormatProvider)CultureInfo.InvariantCulture)));
        }

        internal static ArgumentOutOfRangeException InvalidDestinationBufferIndex(
          int maxLen,
          int dstOffset,
          string parameterName)
        {
            return ADP.ArgumentOutOfRange(SR.Format(SR.ADP_InvalidDestinationBufferIndex, (object)maxLen.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)dstOffset.ToString((IFormatProvider)CultureInfo.InvariantCulture)), parameterName);
        }

        internal static void BuildSchemaTableInfoTableNames(string[] columnNameArray)
        {
            Dictionary<string, int> hash = new Dictionary<string, int>(columnNameArray.Length);
            int val1 = columnNameArray.Length;
            for (int index = columnNameArray.Length - 1; 0 <= index; --index)
            {
                string columnName = columnNameArray[index];
                if (columnName != null && 0 < columnName.Length)
                {
                    string lowerInvariant = columnName.ToLowerInvariant();
                    int val2;
                    if (hash.TryGetValue(lowerInvariant, out val2))
                        val1 = Math.Min(val1, val2);
                    hash[lowerInvariant] = index;
                }
                else
                {
                    columnNameArray[index] = string.Empty;
                    val1 = index;
                }
            }
            int uniqueIndex = 1;
            for (int index = val1; index < columnNameArray.Length; ++index)
            {
                string columnName = columnNameArray[index];
                if (columnName.Length == 0)
                {
                    columnNameArray[index] = "Column";
                    uniqueIndex = ADP.GenerateUniqueName(hash, ref columnNameArray[index], index, uniqueIndex);
                }
                else
                {
                    string lowerInvariant = columnName.ToLowerInvariant();
                    if (index != hash[lowerInvariant])
                        ADP.GenerateUniqueName(hash, ref columnNameArray[index], index, 1);
                }
            }
        }

        private static int GenerateUniqueName(
          Dictionary<string, int> hash,
          ref string columnName,
          int index,
          int uniqueIndex)
        {
            string str;
            string lowerInvariant;
            while (true)
            {
                str = columnName + uniqueIndex.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                lowerInvariant = str.ToLowerInvariant();
                if (hash.ContainsKey(lowerInvariant))
                    ++uniqueIndex;
                else
                    break;
            }
            columnName = str;
            hash.Add(lowerInvariant, index);
            return uniqueIndex;
        }

        internal static bool IsCatchableExceptionType(Exception e) => !(e is NullReferenceException);

        internal enum InternalErrorCode
        {
            UnpooledObjectHasOwner = 0,
            UnpooledObjectHasWrongOwner = 1,
            PushingObjectSecondTime = 2,
            PooledObjectHasOwner = 3,
            PooledObjectInPoolMoreThanOnce = 4,
            CreateObjectReturnedNull = 5,
            NewObjectCannotBePooled = 6,
            NonPooledObjectUsedMoreThanOnce = 7,
            AttemptingToPoolOnRestrictedToken = 8,
            ConvertSidToStringSidWReturnedNull = 10, // 0x0000000A
            AttemptingToConstructReferenceCollectionOnStaticObject = 12, // 0x0000000C
            AttemptingToEnlistTwice = 13, // 0x0000000D
            CreateReferenceCollectionReturnedNull = 14, // 0x0000000E
            PooledObjectWithoutPool = 15, // 0x0000000F
            UnexpectedWaitAnyResult = 16, // 0x00000010
            SynchronousConnectReturnedPending = 17, // 0x00000011
            CompletedConnectReturnedPending = 18, // 0x00000012
            NameValuePairNext = 20, // 0x00000014
            InvalidParserState1 = 21, // 0x00000015
            InvalidParserState2 = 22, // 0x00000016
            InvalidParserState3 = 23, // 0x00000017
            InvalidBuffer = 30, // 0x0000001E
            UnimplementedSMIMethod = 40, // 0x00000028
            InvalidSmiCall = 41, // 0x00000029
            SqlDependencyObtainProcessDispatcherFailureObjectHandle = 50, // 0x00000032
            SqlDependencyProcessDispatcherFailureCreateInstance = 51, // 0x00000033
            SqlDependencyProcessDispatcherFailureAppDomain = 52, // 0x00000034
            SqlDependencyCommandHashIsNotAssociatedWithNotification = 53, // 0x00000035
            UnknownTransactionFailure = 60, // 0x0000003C
        }
    }
}
