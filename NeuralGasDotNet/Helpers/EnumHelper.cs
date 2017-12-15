using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace NeuralGasDotNet.Helpers
{
    public static class EnumHelper
    {
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (nAttributes.Any())
                return (nAttributes.First() as DescriptionAttribute).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            var oTi = CultureInfo.CurrentCulture.TextInfo;
            return oTi.ToTitleCase(oTi.ToLower(eValue.ToString().Replace("_", " ")));
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAllValuesAndDescriptions<TEnum>()
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            return from e in Enum.GetValues(typeof(TEnum)).Cast<Enum>()
                select new KeyValuePair<string, string>(e.ToString(), e.Description());
        }
    }

    public class ValueDescription
    {
        public object Value { get; set; }
        public object Description { get; set; }
    }
}