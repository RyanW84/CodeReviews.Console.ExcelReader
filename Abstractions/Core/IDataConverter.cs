namespace ExcelReader.RyanW84.Abstractions.Core;

/// <summary>
/// Interface for data conversion operations
/// </summary>
public interface IDataConverter<TSource, TTarget>
{
    TTarget Convert(TSource source);
    Task<TTarget> ConvertAsync(TSource source);
}