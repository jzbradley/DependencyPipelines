using System;

public class MissingRequiredDependencyException<T> : Exception
{
    public MissingRequiredDependencyException() : base("Missing dependency " + typeof(T).FullName) { }
}