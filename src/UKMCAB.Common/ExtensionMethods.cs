﻿using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using UKMCAB.Common.Security;

namespace UKMCAB.Common;

//TODO : Needs tobe splint according to the datatype
public static class ExtensionMethods
{
    /// <summary>
    /// Mutates an object if condition; otherwise returns the original model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static T AdaptIf<T>(this T model, bool condition, Func<T, T> action)
    {
        return Pred.AdaptIf(condition, model, action);
    }

    public static string EnsureStartsWith(this string text, string prepender) => text.StartsWith(prepender) ? text : string.Concat(prepender, text);

    public static string? AppendIf(this string? text, string appender) => text.Clean() != null ? text + appender : null;

    /// <summary>
    /// Cleans the email address input, returns it lower cased and trimmed.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string? AsEmailAddress(this string? text) => text?.Clean()?.Trim()?.ToLower();

    /// <summary>
    /// Ensures the DateTime's kind is specified as UTC
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime AsUtc(this DateTime dateTime) => dateTime.Kind == DateTimeKind.Utc ? dateTime : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    public static string ToStringBeisFormat(this DateTime dateTime)
    {
       return dateTime.ToString("dd/MM/yyyy HH:mm");
    }
    public static string ToStringBeisFormat(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
    }
    public static string ToStringBeisDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("d MMM yyyy");
    }
    public static string ToStringBeisDateFormat(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToString("d MMM yyyy") : string.Empty;
    }

    public static string ToStringBeisDateFormat(this string? dateTime)
    {
        var isValidDate = DateOnly.TryParse(dateTime, out var dateOnly);
        return isValidDate ? dateOnly.ToString("d MMM yyyy") : string.Empty;
    }
    public static string AsYesNo(this bool b)
    {
        return b ? "yes" : "no";
    }

    public static string AsYesNo(this bool? b)
    {
        switch (b)
        {
            case true:
                return "Yes";

            case false:
                return "No";

            default:
                return "Unknown";
        }
    }

    public static string AsYesNoOrNotProvided(this bool? b)
    {
        switch (b)
        {
            case true:
                return "Yes";

            case false:
                return "No";

            default:
                return "Not provided";
        }
    }

    /// <summary>
    /// Returns whether the supplied value is between the min/max provided (INCLUSIVE)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool Between(this int? value, int? min, int? max) => value >= min && value <= max;

    public static string? Bracketed(this string? text) => text.Clean() != null ? text.PrependIf("(").AppendIf(")") : null;

    public static T Chain<T>(this T obj, params Action<T>[] actions)
    {
        foreach (var action in actions)
        {
            action(obj);
        }
        return obj;
    }
    public static string? Clean(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        else return text.Trim();
    }

    public static bool Contains(this string? source, string? text, StringComparison stringComparison) => source?.IndexOf(text, stringComparison) > -1;

    public static bool DoesContain(this string? source, string? text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) => (source ?? string.Empty).Contains(text, stringComparison);

    public static bool DoesEqual(this string? source, string? text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) => (source ?? string.Empty).Equals(text ?? string.Empty, stringComparison);

    public static bool DoesNotContain(this string? source, string? text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) => !(source ?? string.Empty).Contains(text, stringComparison);

    public static bool DoesNotEqual(this string? source, string? text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) => !source.DoesEqual(text, stringComparison);

    public static string? Ellipsis(this string? text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (text.Length > maxChars) return text[..(maxChars - 3)] + "...";
        return text;
    }

    /// <summary>
    /// Whether the subject is equal to any of the values supplied.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static bool EqualsAny(this object? value, params object[] values)
        => (value == default && values == default)
        || (values != null && values.Any(x => (x == default && value == default) || (x != default && x.Equals(value))));

    public static T2? Get<T1, T2>(this Dictionary<T1, T2>? dict, T1 key) => dict != null && dict.ContainsKey(key) ? dict[key] : default;

    public static T2? Get<T1, T2>(this IDictionary<T1, T2>? dict, T1 key) => dict != null && dict.ContainsKey(key) ? dict[key] : default;

    public static bool IsInteger(this string? data) => data.ToInteger() != null;

    public static bool IsNotNullOrEmpty(this string? text) => !IsNullOrEmpty(text);

    public static bool IsNullOrEmpty(this string? text) => text.Clean() == null;

    public static bool IsValidEmail(this string? text)
    {
        text = text.Clean();
        if (string.IsNullOrWhiteSpace(text)) return false;
        return Regex.IsMatch(text, @"\A\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,24}\b\Z", RegexOptions.IgnoreCase);
    }

    public static bool IsValidEmailOrEmpty(this string? text) => text.Clean() == null ? true : IsValidEmail(text);

    /// <summary>
    /// Maps an object into something else.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="obj"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TOut Map<TIn, TOut>(this TIn obj, Func<TIn, TOut> func) => obj != null ? func(obj) : default;

    public static Task<TOut?> MapAsync<TIn, TOut>(this TIn obj, Func<TIn, Task<TOut?>> func) => obj != null ? func(obj) : Task.FromResult<TOut?>(default);

    public static string? Md5(this string text) => Md5Helper.CalculateMD5(text).Replace("-", string.Empty);

    /// <summary>
    /// Runs an object through the supplied pipes and then returns the original object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="actions"></param>
    /// <returns></returns>
    public static T Pipe<T>(this T obj, params Action<T>[] actions)
    {
        if (obj != null)
        {
            foreach (var a in actions)
            {
                a(obj);
            }
        }
        return obj;
    }

    public static string? PluralIf(this int val, string suffix = "s") => val == 1 ? string.Empty : suffix;

    public static string? PrependIf(this string? text, string prepender) => text.Clean() != null ? prepender + text : null;

    public static T? RemoveFirst<T>(this List<T> list, Predicate<T> predicate)
    {
        var item = list.Find(predicate);
        if (item != null)
        {
            list.Remove(item);
        }
        return item;
    }

    /// <summary>
    /// Serializes an object to indented json.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string Serialize(this object obj) => JsonSerializer.Serialize(obj, JsonUtil.JsonSerializerOptions);

    public static double? ToDouble(this string data)
    {
        if (data == null) return null;
        if (double.TryParse(data, out double temp))
            return temp;
        else
            return null;
    }

    public static Guid? ToGuid(this string? data)
    {
        if (data == null) return null;
        if (Guid.TryParse(data, out Guid temp)) return temp;
        else return null;
    }

    public static int? ToInteger(this string? data)
    {
        if (data == null) return null;
        int temp;
        if (int.TryParse(data, out temp))
            return temp;
        else
            return null;
    }

    public static string? ToSentenceCase(this string? text) => text.Clean() != null ? char.ToUpper(text[0]) + new string(text.ToLower().Skip(1).ToArray()) : null;

    public static TimeSpan? ToTimeSpan(this string text) => text.Clean() != null ? TimeSpan.Parse(text) : null;

    public static TimeSpan ToTimeSpan(this string text, TimeSpan defaultValue) => text.ToTimeSpan() ?? defaultValue;

    public static TimeSpan ToTimeSpanOrZero(this string text) => text.ToTimeSpan() ?? TimeSpan.Zero;

    public static string? ToTitleCase(this string? text) => text.Clean() != null ? System.Globalization.CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(text) : null;

    /// <summary>
    /// Transforms an object into something else.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="incoming"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static TOut Transform<T, TOut>(this T incoming, Func<T, TOut> action) => action(incoming);
}