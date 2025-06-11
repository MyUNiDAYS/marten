﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Baseline;
using NpgsqlTypes;

namespace Marten.Util
{
    public static class TypeMappings
    {
        private static readonly Dictionary<Type, string> PgTypes = new Dictionary<Type, string>
        {
            {typeof (int), "integer"},
            {typeof (long), "bigint"},
            {typeof (Guid), "uuid"},
            {typeof (string), "varchar"},
            {typeof (Boolean), "boolean"},
            {typeof (double), "double precision"},
            {typeof (decimal), "decimal"},
            {typeof (float), "decimal" },
            {typeof (DateTime), "timestamp without time zone" },
            {typeof (DateTimeOffset), "timestamp with time zone"},
            {typeof (IDictionary<,>), "jsonb" },
        };

        private static readonly MethodInfo _getNgpsqlDbTypeMethod;

        static TypeMappings()
        {
            var type = Type.GetType("Npgsql.TypeHandlerRegistry, Npgsql");
            _getNgpsqlDbTypeMethod = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(
                    x =>
                        x.Name == "ToNpgsqlDbType" && x.GetParameters().Count() == 1 &&
                        x.GetParameters().Single().ParameterType == typeof(Type));
        }

        public static string ConvertSynonyms(string type)
        {
            switch (type.ToLower())
            {
                case "character varying":
                case "varchar":
                    return "varchar";

                case "boolean":
                case "bool":
                    return "boolean";

                case "integer":
                    return "int";

                case "integer[]":
                    return "int[]";

                case "decimal":
                case "numeric":
                    return "decimal";

                case "timestamp without time zone":
                    return "timestamp";

                case "timestamp with time zone":
                    return "timestamptz";
            }

            return type;
        }

        public static string ReplaceMultiSpace(this string str, string newStr)
        {
            var regex = new Regex("\\s+");
            return regex.Replace(str, newStr);
        }

        public static string CanonicizeSql(this string sql)
        {
            var replaced = sql
                .Trim()
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Replace('\t', ' ')
                .ReplaceMultiSpace(" ")
                .Replace("SECURITY INVOKER", "", StringComparison.OrdinalIgnoreCase)
                .Replace("  ", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("LANGUAGE plpgsql AS $function$", "", StringComparison.OrdinalIgnoreCase)
                .Replace("$$ LANGUAGE plpgsql", "$function$", StringComparison.OrdinalIgnoreCase)
                .Replace("AS $$ DECLARE", "DECLARE", StringComparison.OrdinalIgnoreCase)
                .Replace("character varying", "varchar", StringComparison.OrdinalIgnoreCase)
                .Replace("Boolean", "boolean", StringComparison.OrdinalIgnoreCase)
                .Replace("bool,", "boolean,", StringComparison.OrdinalIgnoreCase)
                .Replace("int[]", "integer[]", StringComparison.OrdinalIgnoreCase)
                .Replace("numeric", "decimal", StringComparison.OrdinalIgnoreCase)
                .TrimEnd(';')
                .TrimEnd();

            if (replaced.Contains("PLV8", StringComparison.OrdinalIgnoreCase))
            {
                replaced = replaced
                    .Replace("LANGUAGE plv8 IMMUTABLE STRICT AS $function$", "AS $$", StringComparison.OrdinalIgnoreCase);

                const string languagePlv8ImmutableStrict = "$$ LANGUAGE plv8 IMMUTABLE STRICT";
                const string functionMarker = "$function$";
                if (replaced.EndsWith(functionMarker))
                {
                    replaced = replaced.Substring(0, replaced.LastIndexOf(functionMarker, StringComparison.Ordinal)) + languagePlv8ImmutableStrict;
                }
            }

            return replaced
                .Replace("  ", " ", StringComparison.Ordinal).TrimEnd().TrimEnd(';');
        }

        public static NpgsqlDbType ToDbType(Type type)
        {
            if (type.IsNullable()) return ToDbType(type.GetInnerTypeFromNullable());

            return (NpgsqlDbType)_getNgpsqlDbTypeMethod.Invoke(null, new object[] { type });
        }

        public static string GetPgType(Type memberType)
        {
            if (memberType.GetTypeInfo().IsEnum) return "integer";

            if (memberType.IsArray)
            {
                return GetPgType(memberType.GetElementType()) + "[]";
            }

            if (memberType.IsNullable())
            {
                return GetPgType(memberType.GetInnerTypeFromNullable());
            }

            if (memberType.IsConstructedGenericType)
            {
                var templateType = memberType.GetGenericTypeDefinition();

                if (PgTypes.ContainsKey(templateType)) return PgTypes[templateType];

                return "jsonb";
            }

            return PgTypes.ContainsKey(memberType) ? PgTypes[memberType] : "jsonb";
        }

        public static bool HasTypeMapping(Type memberType)
        {
            if (memberType.IsNullable())
            {
                return HasTypeMapping(memberType.GetInnerTypeFromNullable());
            }

            // more complicated later
            return PgTypes.ContainsKey(memberType) || memberType.GetTypeInfo().IsEnum;
        }

        public static string ApplyCastToLocator(this string locator, EnumStorage enumStyle, Type memberType)
        {
            if (memberType.GetTypeInfo().IsEnum)
            {
                return enumStyle == EnumStorage.AsInteger ? "({0})::int".ToFormat(locator) : locator;
            }

            // Treat "unknown" PgTypes as jsonb (this way null checks of arbitary depth won't fail on cast).
            return "CAST({0} as {1})".ToFormat(locator, GetPgType(memberType));
        }

        public static bool IsDate(this object value)
        {
            if (value == null) return false;

            var type = value.GetType();

            return type == typeof(DateTime) || type == typeof(DateTime?);
        }
    }
}