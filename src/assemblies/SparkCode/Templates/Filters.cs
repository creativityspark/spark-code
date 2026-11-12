using Fluid;
using Fluid.Values;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace SparkCode.Templates
{
    /// <summary>
    /// Implements the custom Liquid filters supported by the template renderer.
    /// </summary>
    internal static class Filters
    {
        /// <summary>
        /// Creates Fluid options with the supported custom filters registered.
        /// </summary>
        /// <returns>Template options configured with the custom filters.</returns>
        internal static TemplateOptions CreateTemplateOptions()
        {
            var options = new TemplateOptions();
            options.Filters.AddFilter("date", FormatDate);
            options.Filters.AddFilter("date_add_days", AddDays);
            options.Filters.AddFilter("date_add_hours", AddHours);
            options.Filters.AddFilter("date_add_minutes", AddMinutes);
            options.Filters.AddFilter("date_add_months", AddMonths);
            options.Filters.AddFilter("date_add_seconds", AddSeconds);
            options.Filters.AddFilter("date_add_years", AddYears);
            options.Filters.AddFilter("date_to_iso8601", FormatIso8601);
            options.Filters.AddFilter("date_to_rfc822", FormatRfc822);
            options.Filters.AddFilter("base64_encode", Base64Encode);
            options.Filters.AddFilter("base64_decode", Base64Decode);
            return options;
        }

        private static ValueTask<FluidValue> Base64Encode(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(input.ToStringValue()));
            return new ValueTask<FluidValue>(new StringValue(encoded));
        }

        private static ValueTask<FluidValue> Base64Decode(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(input.ToStringValue()));
            return new ValueTask<FluidValue>(new StringValue(decoded));
        }

        private static ValueTask<FluidValue> FormatDate(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var date = GetDateTime(input);
            var format = arguments.At(0).ToStringValue();
            return new ValueTask<FluidValue>(
                new StringValue(date.ToString(format, context.CultureInfo)));
        }

        private static ValueTask<FluidValue> AddDays(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            return AddDate(input, value => value.AddDays(GetNumber(arguments)), context);
        }

        private static ValueTask<FluidValue> AddHours(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            return AddDate(input, value => value.AddHours(GetNumber(arguments)), context);
        }

        private static ValueTask<FluidValue> AddMinutes(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            return AddDate(input, value => value.AddMinutes(GetNumber(arguments)), context);
        }

        private static ValueTask<FluidValue> AddMonths(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var months = GetWholeNumber(arguments);
            return AddDate(input, value => value.AddMonths(months), context);
        }

        private static ValueTask<FluidValue> AddSeconds(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            return AddDate(input, value => value.AddSeconds(GetNumber(arguments)), context);
        }

        private static ValueTask<FluidValue> AddYears(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var years = GetWholeNumber(arguments);
            return AddDate(input, value => value.AddYears(years), context);
        }

        private static ValueTask<FluidValue> FormatIso8601(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var formatted = GetDateTime(input)
                .ToUniversalTime()
                .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            return new ValueTask<FluidValue>(new StringValue(formatted));
        }

        private static ValueTask<FluidValue> FormatRfc822(
            FluidValue input,
            FilterArguments arguments,
            TemplateContext context)
        {
            var formatted = GetDateTime(input)
                .ToUniversalTime()
                .ToString("ddd, dd MMM yyyy HH:mm:ss 'Z'", CultureInfo.InvariantCulture);
            return new ValueTask<FluidValue>(new StringValue(formatted));
        }

        private static ValueTask<FluidValue> AddDate(
            FluidValue input,
            Func<DateTimeOffset, DateTimeOffset> add,
            TemplateContext context)
        {
            return new ValueTask<FluidValue>(
                FluidValue.Create(add(GetDateTime(input)), context.Options));
        }

        private static DateTimeOffset GetDateTime(FluidValue input)
        {
            var value = input.ToObjectValue();

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.Kind == DateTimeKind.Unspecified
                    ? new DateTimeOffset(dateTime, TimeSpan.Zero)
                    : new DateTimeOffset(dateTime);
            }

            var dateText = value as string ?? input.ToStringValue();
            if (DateTimeOffset.TryParse(
                dateText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out dateTimeOffset))
            {
                return dateTimeOffset;
            }

            throw new FormatException($"The value '{dateText}' is not a valid date/time.");
        }

        private static double GetNumber(FilterArguments arguments)
        {
            var value = arguments.At(0).ToObjectValue();
            if (value == null)
            {
                throw new FormatException("Date arithmetic requires a numeric argument.");
            }

            var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                throw new FormatException("Date arithmetic requires a finite number.");
            }

            return number;
        }

        private static int GetWholeNumber(FilterArguments arguments)
        {
            var number = GetNumber(arguments);
            if (number % 1 != 0 || number < int.MinValue || number > int.MaxValue)
            {
                throw new FormatException("Month and year date arithmetic requires a whole number.");
            }

            return (int)number;
        }
    }
}
