namespace CronBlocks.SerialPortInterface.Interfaces;

/// <summary>
/// Provides an easy interface for working with Serial port's
/// type conversions.
/// </summary>
public interface ISerialOptionsService
{
    /// <summary>
    /// Returns a list of strings containing all the options that are
    /// available for a type in serial definition.
    ///   For example: use
    ///         GetAllOptions<BaudRate>
    ///         GetAllOptions<DataBits>
    ///         GetAllOptions<Parity>
    ///         GetAllOptions<StopBits>
    ///      to get all the available options.
    /// </summary>
    /// <typeparam name="T">BaudRate,DataBits,Parity,StopBits are supported</typeparam>
    /// <returns>All the available options for a type</returns>
    IEnumerable<string> GetAllOptions<T>();
    /// <summary>
    /// Converts an option string back to the specified type.
    ///   For example: BaudRate br = ConvertOption<BaudRate>("1200");
    /// </summary>
    /// <typeparam name="T">BaudRate,DataBits,Parity,StopBits are supported</typeparam>
    /// <param name="option"></param>
    /// <returns>The strongly typed value</returns>
    T ConvertOption<T>(string option);
}