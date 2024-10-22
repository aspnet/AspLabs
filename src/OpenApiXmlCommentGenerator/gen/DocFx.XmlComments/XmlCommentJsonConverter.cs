using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocFx.XmlComments;

internal sealed class XmlCommentJsonConverter : JsonConverter<XmlComment>
{
    public override XmlComment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var xmlComment = new XmlComment();
        reader.Read();
        do
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString() ?? throw new JsonException();
                    ReadProperty(ref reader, propertyName, xmlComment, options);
                    break;
                default:
                    continue;
            }
        } while (reader.Read());


        return xmlComment;
    }

    internal static void ReadProperty(ref Utf8JsonReader reader, string propertyName, XmlComment xmlComment, JsonSerializerOptions options)
    {
        switch (propertyName)
        {
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Summary)) ?? "Summary") == propertyName:
                reader.Read();
                xmlComment.Summary = reader.GetString();
                break;
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Description)) ?? "Description") == propertyName:
                reader.Read();
                xmlComment.Description = reader.GetString();
                break;
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Parameters)) ?? "Parameters") == propertyName:
                reader.Read();
                xmlComment.Parameters = ReadParametersProperty(ref reader, options);
                break;
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Responses)) ?? "Responses") == propertyName:
                reader.Read();
                xmlComment.Responses = ReadResponsesProperty(ref reader, options);
                break;
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Parameters)) ?? "Returns") == propertyName:
                reader.Read();
                xmlComment.Returns = reader.GetString();
                break;
            case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Examples)) ?? "Examples") == propertyName:
                reader.Read();
                reader.Read();
                xmlComment.Examples = [];
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    xmlComment.Examples.Add(reader.GetString());
                    reader.Read();
                }
                break;
            default:
                break;
        }
    }

    internal static List<XmlParameterComment> ReadParametersProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        List<XmlParameterComment> result = [];
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();
        var xmlParameterComment = new XmlParameterComment();
        do
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    xmlParameterComment = new XmlParameterComment();
                    break;
                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString();
                    switch (propertyName)
                    {
                        case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlParameterComment.Name)) ?? "Name") == propertyName:
                            reader.Read();
                            xmlParameterComment.Name = reader.GetString()!;
                            break;
                        case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlResponseComment.Description)) ?? "Description") == propertyName:
                            reader.Read();
                            xmlParameterComment.Description = reader.GetString();
                            break;
                        case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlParameterComment.Example)) ?? "Example") == propertyName:
                            reader.Read();
                            xmlParameterComment.Example = reader.GetString();
                            break;
                        default:
                            break;
                    }
                    break;
                case JsonTokenType.EndObject:
                    result.Add(xmlParameterComment);
                    break;
                case JsonTokenType.EndArray:
                    return result;
                default:
                    break;
            }
        } while (reader.Read());
        return result;
    }

    internal static List<XmlResponseComment> ReadResponsesProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        List<XmlResponseComment> result = [];
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();
        var xmlResponseComment = new XmlResponseComment();
        string? code = null;
        do
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    xmlResponseComment = new XmlResponseComment();
                    code = null;
                    break;
                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString();
                    switch (propertyName)
                    {
                        case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlResponseComment.Description)) ?? "Description") == propertyName:
                            reader.Read();
                            xmlResponseComment.Description = reader.GetString();
                            break;
                        case string when (options.PropertyNamingPolicy?.ConvertName(nameof(XmlResponseComment.Code)) ?? "Code") == propertyName:
                            reader.Read();
                            xmlResponseComment.Code = reader.GetString()!;
                            code = xmlResponseComment.Code;
                            break;
                        default:
                            break;
                    }
                    break;
                case JsonTokenType.EndObject:
                    Debug.Assert(code != null, "Response code should not be unset yet.");
                    result.Add(xmlResponseComment);
                    break;
                case JsonTokenType.EndArray:
                    return result;
                default:
                    continue;
            }
        } while (reader.Read());
        return result;
    }

    public override void Write(Utf8JsonWriter writer, XmlComment value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Summary)) ?? nameof(XmlComment.Summary));
        writer.WriteStringValue(value.Summary);
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Description)) ?? nameof(XmlComment.Description));
        writer.WriteStringValue(value.Description);
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Remarks)) ?? nameof(XmlComment.Remarks));
        writer.WriteStringValue(value.Remarks);
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Returns)) ?? nameof(XmlComment.Returns));
        writer.WriteStringValue(value.Returns);
        if (value.Examples is { Count: > 0 } examples)
        {
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Examples)) ?? nameof(XmlComment.Examples));
            writer.WriteStartArray();
            foreach (var example in examples)
            {
                writer.WriteStringValue(example);
            }
            writer.WriteEndArray();
        }
        if (value.Parameters is { Count: > 0 } parameters)
        {
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Parameters)) ?? nameof(XmlComment.Parameters));
            writer.WriteStartArray();
            foreach (var parameter in parameters)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlParameterComment.Name)) ?? nameof(XmlParameterComment.Name));
                writer.WriteStringValue(parameter.Name);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlParameterComment.Description)) ?? nameof(XmlParameterComment.Description));
                writer.WriteStringValue(parameter.Description);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlParameterComment.Example)) ?? nameof(XmlParameterComment.Example));
                writer.WriteStringValue(parameter.Example);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        if (value.Responses is { Count: > 0 } responses)
        {
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlComment.Responses)) ?? nameof(XmlComment.Responses));
            writer.WriteStartArray();
            foreach (var response in responses)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlResponseComment.Code)) ?? nameof(XmlResponseComment.Code));
                writer.WriteStringValue(response.Code);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(XmlResponseComment.Description)) ?? nameof(XmlResponseComment.Description));
                writer.WriteStringValue(response.Description);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
