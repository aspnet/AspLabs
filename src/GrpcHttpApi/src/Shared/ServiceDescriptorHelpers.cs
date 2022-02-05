// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Google.Api;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace Grpc.Shared.HttpApi
{
    internal static class ServiceDescriptorHelpers
    {
        public static ServiceDescriptor? GetServiceDescriptor(Type serviceReflectionType)
        {
            var property = serviceReflectionType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
            if (property != null)
            {
                return (ServiceDescriptor?)property.GetValue(null);
            }

            throw new InvalidOperationException($"Get not find Descriptor property on {serviceReflectionType.Name}.");
        }

        public static bool TryResolveDescriptors(MessageDescriptor messageDescriptor, string variable, [NotNullWhen(true)]out List<FieldDescriptor>? fieldDescriptors)
        {
            fieldDescriptors = null;
            var path = variable.AsSpan();
            MessageDescriptor? currentDescriptor = messageDescriptor;

            while (path.Length > 0)
            {
                var separator = path.IndexOf('.');

                string fieldName;
                if (separator != -1)
                {
                    fieldName = path.Slice(0, separator).ToString();
                    path = path.Slice(separator + 1);
                }
                else
                {
                    fieldName = path.ToString();
                    path = ReadOnlySpan<char>.Empty;
                }

                var field = currentDescriptor?.FindFieldByName(fieldName);
                if (field == null)
                {
                    fieldDescriptors = null;
                    return false;
                }

                if (fieldDescriptors == null)
                {
                    fieldDescriptors = new List<FieldDescriptor>();
                }

                fieldDescriptors.Add(field);
                if (field.FieldType == FieldType.Message)
                {
                    currentDescriptor = field.MessageType;
                }
                else
                {
                    currentDescriptor = null;
                }

            }

            return fieldDescriptors != null;
        }

        private static object ConvertValue(object value, FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Double:
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                case FieldType.Float:
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                case FieldType.Int64:
                case FieldType.SInt64:
                case FieldType.SFixed64:
                    return Convert.ToInt64(value, CultureInfo.InvariantCulture);
                case FieldType.UInt64:
                case FieldType.Fixed64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                case FieldType.Int32:
                case FieldType.SInt32:
                case FieldType.SFixed32:
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                case FieldType.Bool:
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                case FieldType.String:
                    return value;
                case FieldType.Bytes:
                    {
                        if (value is string s)
                        {
                            return ByteString.FromBase64(s);
                        }
                        throw new InvalidOperationException("Base64 encoded string required to convert to bytes.");
                    }
                case FieldType.UInt32:
                case FieldType.Fixed32:
                    return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                case FieldType.Enum:
                    {
                        if (value is string s)
                        {
                            var enumValueDescriptor = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                                ? descriptor.EnumType.FindValueByNumber(i)
                                : descriptor.EnumType.FindValueByName(s);

                            if (enumValueDescriptor == null)
                            {
                                throw new InvalidOperationException($"Invalid value '{s}' for enum type {descriptor.EnumType.Name}.");
                            }

                            return enumValueDescriptor.Number;
                        }
                        throw new InvalidOperationException("String required to convert to enum.");
                    }
                case FieldType.Message:
                    if (IsWrapperType(descriptor.MessageType))
                    {
                        return ConvertValue(value, descriptor.MessageType.FindFieldByName("value"));
                    }
                    break;
            }

            throw new InvalidOperationException("Unsupported type: " + descriptor.FieldType);
        }

        public static void RecursiveSetValue(IMessage currentValue, List<FieldDescriptor> pathDescriptors, object values)
        {
            for (var i = 0; i < pathDescriptors.Count; i++)
            {
                var isLast = i == pathDescriptors.Count - 1;
                var field = pathDescriptors[i];

                if (isLast)
                {
                    if (field.IsRepeated)
                    {
                        var list = (IList)field.Accessor.GetValue(currentValue);
                        if (values is StringValues stringValues)
                        {
                            foreach (var value in stringValues)
                            {
                                list.Add(ConvertValue(value, field));
                            }
                        }
                        else if (values is IList listValues)
                        {
                            foreach (var value in listValues)
                            {
                                list.Add(ConvertValue(value, field));
                            }
                        }
                        else
                        {
                            list.Add(ConvertValue(values, field));
                        }
                    }
                    else
                    {
                        if (values is StringValues stringValues)
                        {
                            if (stringValues.Count == 1)
                            {
                                field.Accessor.SetValue(currentValue, ConvertValue(stringValues[0], field));
                            }
                            else
                            {
                                throw new InvalidOperationException("Can't set multiple values onto a non-repeating field.");
                            }
                        }
                        else if (values is IMessage message)
                        {
                            field.Accessor.SetValue(currentValue, message);
                        }
                        else
                        {
                            field.Accessor.SetValue(currentValue, ConvertValue(values, field));
                        }
                    }
                }
                else
                {
                    var fieldMessage = (IMessage)field.Accessor.GetValue(currentValue);

                    if (fieldMessage == null)
                    {
                        fieldMessage = (IMessage)Activator.CreateInstance(field.MessageType.ClrType)!;
                        field.Accessor.SetValue(currentValue, fieldMessage);
                    }

                    currentValue = fieldMessage;
                }
            }
        }

        internal static bool IsWrapperType(MessageDescriptor m) => m.File.Package == "google.protobuf" && m.File.Name == "google/protobuf/wrappers.proto";

        public static bool TryGetHttpRule(MethodDescriptor methodDescriptor, [NotNullWhen(true)]out HttpRule? httpRule)
        {
            // Protobuf id of the HttpRule field
            const int HttpRuleFieldId = 72295728;

            // CustomOptions is obsolete
            // We can use `methodDescriptor.GetOption(AnnotationsExtensions.Http)` but there
            // is an error thrown when there is no option on the method.
            // TODO(JamesNK): Remove obsolete code when issue is fixed. https://github.com/protocolbuffers/protobuf/issues/7127

#pragma warning disable CS0618 // Type or member is obsolete
            return methodDescriptor.CustomOptions.TryGetMessage<HttpRule>(HttpRuleFieldId, out httpRule);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static bool TryResolvePattern(HttpRule http, [NotNullWhen(true)]out string? pattern, [NotNullWhen(true)]out string? verb)
        {
            switch (http.PatternCase)
            {
                case HttpRule.PatternOneofCase.Get:
                    pattern = http.Get;
                    verb = "GET";
                    return true;
                case HttpRule.PatternOneofCase.Put:
                    pattern = http.Put;
                    verb = "PUT";
                    return true;
                case HttpRule.PatternOneofCase.Post:
                    pattern = http.Post;
                    verb = "POST";
                    return true;
                case HttpRule.PatternOneofCase.Delete:
                    pattern = http.Delete;
                    verb = "DELETE";
                    return true;
                case HttpRule.PatternOneofCase.Patch:
                    pattern = http.Patch;
                    verb = "PATCH";
                    return true;
                case HttpRule.PatternOneofCase.Custom:
                    pattern = http.Custom.Path;
                    verb = http.Custom.Kind;
                    return true;
                default:
                    pattern = null;
                    verb = null;
                    return false;
            }
        }

        public static Dictionary<string, List<FieldDescriptor>> ResolveRouteParameterDescriptors(RoutePattern pattern, MessageDescriptor messageDescriptor)
        {
            var routeParameterDescriptors = new Dictionary<string, List<FieldDescriptor>>(StringComparer.Ordinal);
            foreach (var routeParameter in pattern.Parameters)
            {
                if (!TryResolveDescriptors(messageDescriptor, routeParameter.Name, out var fieldDescriptors))
                {
                    throw new InvalidOperationException($"Couldn't find matching field for route parameter '{routeParameter.Name}' on {messageDescriptor.Name}.");
                }

                routeParameterDescriptors.Add(routeParameter.Name, fieldDescriptors);
            }

            return routeParameterDescriptors;
        }

        public static BodyDescriptorInfo? ResolveBodyDescriptor(string body, Type serviceType, MethodDescriptor methodDescriptor)
        {
            if (!string.IsNullOrEmpty(body))
            {
                if (!string.Equals(body, "*", StringComparison.Ordinal))
                {
                    if (!TryResolveDescriptors(methodDescriptor.InputType, body, out var bodyFieldDescriptors))
                    {
                        throw new InvalidOperationException($"Couldn't find matching field for body '{body}' on {methodDescriptor.InputType.Name}.");
                    }
                    var leafDescriptor = bodyFieldDescriptors.Last();
                    var propertyName = FormatUnderscoreName(leafDescriptor.Name, pascalCase: true, preservePeriod: false);
                    var propertyInfo = leafDescriptor.ContainingType.ClrType.GetProperty(propertyName);

                    if (leafDescriptor.IsRepeated)
                    {
                        // A repeating field isn't a message type. The JSON parser will parse using the containing
                        // type to get the repeating collection.
                        return new BodyDescriptorInfo(leafDescriptor.ContainingType, bodyFieldDescriptors, IsDescriptorRepeated: true, propertyInfo);
                    }
                    else
                    {
                        return new BodyDescriptorInfo(leafDescriptor.MessageType, bodyFieldDescriptors, IsDescriptorRepeated: false, propertyInfo);
                    }
                }
                else
                {
                    ParameterInfo? requestParameter = null;
                    var methodInfo = serviceType.GetMethod(methodDescriptor.Name);
                    if (methodInfo != null)
                    {
                        requestParameter = methodInfo.GetParameters().SingleOrDefault(p => p.Name == "request");
                    }

                    return new BodyDescriptorInfo(methodDescriptor.InputType, FieldDescriptors: null, IsDescriptorRepeated: false, ParameterInfo: requestParameter);
                }
            }

            return null;
        }

        public record BodyDescriptorInfo(
            MessageDescriptor Descriptor,
            List<FieldDescriptor>? FieldDescriptors,
            bool IsDescriptorRepeated,
            PropertyInfo? PropertyInfo = null,
            ParameterInfo? ParameterInfo = null);

        public static string FormatUnderscoreName(string input, bool pascalCase, bool preservePeriod)
        {
            var capitalizeNext = pascalCase;
            var result = string.Empty;

            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsLower(input[i]))
                {
                    if (capitalizeNext)
                    {
                        result += char.ToUpper(input[i]);
                    }
                    else
                    {
                        result += input[i];
                    }
                    capitalizeNext = false;
                }
                else if (char.IsUpper(input[i]))
                {
                    if (i == 0 && !capitalizeNext)
                    {
                        // Force first letter to lower-case unless explicitly told to
                        // capitalize it.
                        result += char.ToLower(input[i]);
                    }
                    else
                    {
                        // Capital letters after the first are left as-is.
                        result += input[i];
                    }
                    capitalizeNext = false;
                }
                else if (char.IsDigit(input[i]))
                {
                    result += input[i];
                    capitalizeNext = true;
                }
                else
                {
                    capitalizeNext = true;
                    if (input[i] == '.' && preservePeriod)
                    {
                        result += '.';
                    }
                }
            }
            // Add a trailing "_" if the name should be altered.
            if (input.Length > 0 && input[input.Length - 1] == '#')
            {
                result += '_';
            }
            return result;
        }
    }
}
