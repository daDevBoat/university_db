using System;
namespace UniversityDBApp.integration;

public class NoTransactionStartedException : Exception
{
    public NoTransactionStartedException() : base("No transaction is started") {}
    public NoTransactionStartedException(string message) : base(message) {}
    public NoTransactionStartedException(Exception e) : base("No transaction is started", e) {}
    public NoTransactionStartedException(string message, Exception e) : base(message, e) {}
}

public class TransactionAlreadyStartedException : Exception
{
    public TransactionAlreadyStartedException() : base("Transaction has already started started. Cannot start a new one.") {}
    public TransactionAlreadyStartedException(string message) : base(message) {}
    public TransactionAlreadyStartedException(Exception e) : base("Transaction has already started started. Cannot start a new one.", e) {}
    public TransactionAlreadyStartedException(string message, Exception e) : base(message, e) {}
}

public class DBUpdateFailedException : Exception
{
    public DBUpdateFailedException() : base("The update to the DB failed") {}
    public DBUpdateFailedException(string message) : base(message) {}
    public DBUpdateFailedException(Exception e) : base("The update to the DB failed", e) {}
    public DBUpdateFailedException(string message, Exception e) : base(message, e) {}
}
public class SelectForUpdateIsNullException : Exception
{
    public SelectForUpdateIsNullException() : base("The select for update returned null") {}
    public SelectForUpdateIsNullException(string message) : base(message) {}
    public SelectForUpdateIsNullException(Exception e) : base("The select for update returned null", e) {}
    public SelectForUpdateIsNullException(string message, Exception e) : base(message, e) {}
}

