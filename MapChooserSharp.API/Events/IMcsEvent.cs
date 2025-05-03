namespace MapChooserSharp.API.Events;

/// <summary>
/// All of Mcs event inherit this interface
/// </summary>
public interface IMcsEvent;


/// <summary>
/// If event accepts a result, it should implement this interface.
/// </summary>
public interface IMcsEventWithResult : IMcsEvent;

/// <summary>
/// If event not accepts a result, it should implement this interface
/// </summary>
public interface IMcsEventNoResult : IMcsEvent;