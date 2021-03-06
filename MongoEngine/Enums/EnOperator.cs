﻿/// <summary>
/// Logic operation
/// </summary>
internal enum EnOperator
{
    /// <summary>
    /// Must have this value
    /// </summary>
    And,
    /// <summary>
    /// Should have this value
    /// </summary>
    Or,
    /// <summary>
    /// Must not have this value
    /// </summary>
    Not,
    /// <summary>
    /// The main criterion will behave like And Operator
    /// </summary>
    Find
}