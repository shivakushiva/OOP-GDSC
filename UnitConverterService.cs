//READ ME:
// THE OOP PROJECT THAT I DEMONSTRATED IS FROM A COLLEAGUE
// HE ONLY ALLOWS TO USE THE FILES THAT ARE DEMONSTRATED THE VIDEO
// SO THERE WILL BE LOT OF SQUIGGLY LINES WHEN LAUNCHED ON VISUAL STUDIO
// THANKS FOR UNDERSTANDING

using Helpers.CustomUnits.CurrencyUnits;
using Helpers.CustomUnits.NumberSystemUnits;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UnitsNet;
using UnitsNet.Units;

namespace Services.UnitConverterServices;

public partial class UnitConverterService
{
    private static HttpClient _client;

    public UnitConverterService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient();
    }

    public async Task<string> ConvertUnits(StringBuilder equationStringBuilder,
        Enum fromUnit,
        Enum toUnit)
    {
        if (fromUnit is NumberSystemUnit originNumberSystemUnit && toUnit is NumberSystemUnit toNumberSystemUnit)
        {
            var convertFrom = new NumberSystem(equationStringBuilder.ToString(), originNumberSystemUnit);
            var numberSystemConverterDelegate = UnitConverter.Default.GetConversionFunction<NumberSystem>(originNumberSystemUnit, toNumberSystemUnit);
            var convertedNumberSystemUnit = ((NumberSystem)numberSystemConverterDelegate(convertFrom)).Value;

            return convertedNumberSystemUnit;
        }
        else if (fromUnit is CurrencyConverterUnit originCurrencyUnit && toUnit is CurrencyConverterUnit toCurrencyUnit)
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var endpointRequest = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = originCurrencyUnit.GetCurrencyEndpoint(toCurrencyUnit)
            }, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            var endpointResponse = await endpointRequest.Content.ReadFromJsonAsync<object>().ConfigureAwait(false);
            var conversionDate = ((JsonElement)endpointResponse).GetProperty("date").GetString();
            var conversionRate = ((JsonElement)endpointResponse).GetProperty(toUnit.ToString().ToLower()).GetDouble();
            var convertedCurrency = double.Parse(equationStringBuilder.ToString()) * conversionRate;

            return $"{conversionDate} {convertedCurrency}";
        }
        else
        {
            //todo: fix here (yung sa undefined)
            var parsedEquation = double.Parse(equationStringBuilder.ToString());
            return UnitConverter.TryConvert(parsedEquation, fromUnit, toUnit, out var convertedUnit)
                ? convertedUnit.ToString()
                : throw new ArgumentException($"Unhandled value: {convertedUnit}");
        }
    }

    /// <summary> Used for units equation conversion prettier </summary>
    /// <param name="equation">The equation to be prettified</param>
    /// <returns>The pretty string (number with comma if it is more than thousand)</returns>
    public string EquationPrettier(string equation)
    {
        if (equation.Contains('.'))
        {
            // Gets all whole numbers
            var wholeNumbers = equation.Split('.')[0];
            // Add commas to whole numbers if it is more than thousand
            var prettifiedWholeNumbers = $"{double.Parse(wholeNumbers):N0}";
            // Get all numbers after the decimal point
            var decimals = equation[(equation.IndexOf('.') + 1)..];
            // Concat pretty whole numbers and decimal numbers
            var prettyEquation = $"{prettifiedWholeNumbers}.{decimals}";
            // Limit the equation to 21 chars to fit the screen
            var truncatedPrettyEquation = prettyEquation[..Math.Min(prettyEquation.Length, 21)];

            return truncatedPrettyEquation;
        }
        else
        {
            // Number is scientific notation
            if (equation.Any(_ => _ == 'E'))
            {
                var prettifiedNumbers = $"{decimal.Parse(equation, NumberStyles.Float)}";
                return prettifiedNumbers;
            }
            // Number is plain
            else
            {
                var prettifiedNumbers = $"{double.Parse(equation):N0}";
                return prettifiedNumbers;
            }
        }
    }

    public IEnumerable<string> GetSIUnitAndNameById(Type unitType)
    {
        var unitNames = GetUnitNames(unitType);
        var siUnits = GetSIUnits(unitType);
        var nameAndSiUnits = unitNames
            .Zip(siUnits, (name, siUnit) => string.IsNullOrWhiteSpace(siUnit) ? name : $"{name} - {siUnit}");

        return nameAndSiUnits;
    }

    public string GetSIUnitById(int unitId, Type unitType)
    {
        var siUnits = GetSIUnits(unitType);
        return siUnits.ElementAt(unitId);
    }

    private IEnumerable<string> GetSIUnits(Type unitType)
    {
        var siUnits = GetUnitInfos(unitType)
            .Select(_ => unitType switch
            {
                var _ when unitType == typeof(AccelerationUnit) => Acceleration.GetAbbreviation((AccelerationUnit)_.Value),
                var _ when unitType == typeof(AreaUnit) => Area.GetAbbreviation((AreaUnit)_.Value),
                var _ when unitType == typeof(BitRateUnit) => BitRate.GetAbbreviation((BitRateUnit)_.Value),
                var _ when unitType == typeof(CapacitanceUnit) => Capacitance.GetAbbreviation((CapacitanceUnit)_.Value),
                var _ when unitType == typeof(CurrencyConverterUnit) => CurrencyUnit.GetAbbrevieation((CurrencyConverterUnit)_.Value),
                var _ when unitType == typeof(DensityUnit) => Density.GetAbbreviation((DensityUnit)_.Value),
                var _ when unitType == typeof(DurationUnit) => Duration.GetAbbreviation((DurationUnit)_.Value),
                var _ when unitType == typeof(ElectricCurrentUnit) => ElectricCurrent.GetAbbreviation((ElectricCurrentUnit)_.Value),
                var _ when unitType == typeof(EnergyUnit) => Energy.GetAbbreviation((EnergyUnit)_.Value),
                var _ when unitType == typeof(EntropyUnit) => Entropy.GetAbbreviation((EntropyUnit)_.Value),
                var _ when unitType == typeof(ForceUnit) => Force.GetAbbreviation((ForceUnit)_.Value),
                var _ when unitType == typeof(InformationUnit) => Information.GetAbbreviation((InformationUnit)_.Value),
                var _ when unitType == typeof(InformationUnit) => Information.GetAbbreviation((InformationUnit)_.Value),
                var _ when unitType == typeof(LengthUnit) => Length.GetAbbreviation((LengthUnit)_.Value),
                var _ when unitType == typeof(MassUnit) => Mass.GetAbbreviation((MassUnit)_.Value),
                var _ when unitType == typeof(NumberSystemUnit) => NumberSystem.GetAbbreviation((NumberSystemUnit)_.Value),
                var _ when unitType == typeof(PowerUnit) => Power.GetAbbreviation((PowerUnit)_.Value),
                var _ when unitType == typeof(PressureUnit) => Pressure.GetAbbreviation((PressureUnit)_.Value),
                var _ when unitType == typeof(SpeedUnit) => Speed.GetAbbreviation((SpeedUnit)_.Value),
                var _ when unitType == typeof(TemperatureUnit) => Temperature.GetAbbreviation((TemperatureUnit)_.Value),
                var _ when unitType == typeof(TorqueUnit) => Torque.GetAbbreviation((TorqueUnit)_.Value),
                var _ when unitType == typeof(VolumeUnit) => Volume.GetAbbreviation((VolumeUnit)_.Value),
                _ => throw new ArgumentException($"Unhandled value: {unitType}")
            });

        return siUnits;
    }

    private UnitInfo[] GetUnitInfos(Type unitType)
    {
        UnitInfo[] units = unitType switch
        {
            var _ when unitType == typeof(AccelerationUnit) => Acceleration.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(AreaUnit) => Area.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(BitRateUnit) => BitRate.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(CapacitanceUnit) => Capacitance.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(CurrencyConverterUnit) => CurrencyUnit.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(DensityUnit) => Density.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(DurationUnit) => Duration.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(ElectricCurrentUnit) => ElectricCurrent.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(EnergyUnit) => Energy.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(EntropyUnit) => Entropy.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(ForceUnit) => Force.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(InformationUnit) => Information.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(InformationUnit) => Information.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(LengthUnit) => Length.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(MassUnit) => Mass.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(NumberSystemUnit) => NumberSystem.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(PowerUnit) => Power.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(PressureUnit) => Pressure.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(SpeedUnit) => Speed.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(SpeedUnit) => Speed.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(TemperatureUnit) => Temperature.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(TorqueUnit) => Torque.Zero.QuantityInfo.UnitInfos,
            var _ when unitType == typeof(VolumeUnit) => Volume.Zero.QuantityInfo.UnitInfos,
            _ => throw new ArgumentException($"Unhandled value: {unitType}")
        };

        return units;
    }

    public string GetUnitNameById(int unitId, Type unitType)
    {
        var units = GetUnitNames(unitType);
        return units.ElementAt(unitId);
    }

    private IEnumerable<string> GetUnitNames(Type unitType)
    {
        Func<UnitInfo, string> correctUnitInfoProperty = _ => unitType == typeof(CurrencyConverterUnit)
            ? _.PluralName
            : _.Name;

        var unitNames = GetUnitInfos(unitType)
            .Select(_ => string.Concat(correctUnitInfoProperty?.Invoke(_)
            .Select(_ => char.IsUpper(_) ? $" {_}" : _.ToString())));

        return unitNames;
    }
}