using System;
using System.IO;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;

namespace AppifySheets.TBC.IntegrationService.Client.TBC_Services;

public static class XmlExtensions
{
    public static Result<T> XmlDeserializeFromString<T>(this string objectData)
    {
        var result = Result.Try(() => (T)objectData.XmlDeserializeFromString(typeof(T)));
        if (result.IsFailure) return result.ConvertFailure<T>();

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (result.Value == null) return Result.Failure<T>("Null value in result.Value");
        return result;
    }

    static object XmlDeserializeFromString(this string objectData, Type type)
    {
        var serializer = new XmlSerializer(type);

        using TextReader reader = new StringReader(objectData);
        var result = serializer.Deserialize(reader);

        return result;
    }
}