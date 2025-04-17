namespace MapChooserSharp.API.Events;


/// <summary>
/// McsEventResult but with callback
/// </summary>
public readonly struct McsEventResultWithCallback
{
    /// <summary>
    /// Event result
    /// </summary>
    public McsEventResult Result { get; }
    
    /// <summary>
    /// Simple callback method
    /// </summary>
    public Action<McsEventResult>? Callback { get; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="result">McsEventResult</param>
    /// <param name="callback">Callback function</param>
    public McsEventResultWithCallback(McsEventResult result, Action<McsEventResult>? callback = null)
    {
        Result = result;
        Callback = callback;
    }
    
    /// <summary>
    /// Simple factory method for making McsEventResultWithCallback
    /// </summary>
    /// <returns>McsEventResultWithCallback object with McsEventResult.Continue</returns>
    public static McsEventResultWithCallback Continue() => new(McsEventResult.Continue);

    /// <summary>
    /// Simple factory method for making McsEventResultWithCallback
    /// </summary>
    /// <returns>McsEventResultWithCallback object with McsEventResult.Changed</returns>
    public static McsEventResultWithCallback Changed() => new(McsEventResult.Changed);

    /// <summary>
    /// Simple factory method for making McsEventResultWithCallback
    /// </summary>
    /// <returns>McsEventResultWithCallback object with McsEventResult.Handled</returns>
    public static McsEventResultWithCallback Handled() => new(McsEventResult.Handled);

    /// <summary>
    /// Factory method for making McsEventResultWithCallback with Stop result
    /// </summary>
    /// <param name="callback">Optional callback function</param>
    /// <returns>McsEventResultWithCallback object with McsEventResult.Stop</returns>
    public static McsEventResultWithCallback Stop(Action<McsEventResult>? callback = null) => 
        new(McsEventResult.Stop, callback);
    
    /// <summary>
    /// Implicit conversion for compatibility with McsEventResult
    /// </summary>
    /// <param name="resultWithCallback">McsEventResultWithCallback instance</param>
    /// <returns>McsEventResult value</returns>
    public static implicit operator McsEventResult(McsEventResultWithCallback resultWithCallback) => 
        resultWithCallback.Result;
    
    /// <summary>
    /// Implicit conversion for compatibility with McsEventResult
    /// </summary>
    /// <param name="result">McsEventResult value</param>
    /// <returns>McsEventResultWithCallback instance</returns>
    public static implicit operator McsEventResultWithCallback(McsEventResult result) => 
        new(result);
}