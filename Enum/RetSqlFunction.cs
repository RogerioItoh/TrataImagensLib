namespace Enum
{
    public enum RetSqlFunction : int
    {
        retSQLOk = 0,
        retSQLErrorLocked = 1,
        retSQLErrorReconnect = 3,
        retSQLErrorTimeOut = 4,
        retSQLDuplicatePK = 5,
        retInvalidUserPwd = 6,
        retVBError = 97,
        retSQLEmpty = 98,
        retSQLError = 99
    }
}
