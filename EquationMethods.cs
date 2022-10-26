//READ ME:
// THE OOP PROJECT THAT I DEMONSTRATED IS FROM A COLLEAGUE
// HE ONLY ALLOWS TO USE THE FILES THAT ARE DEMONSTRATED THE VIDEO
// SO THERE WILL BE LOT OF SQUIGGLY LINES WHEN LAUNCHED ON VISUAL STUDIO
// THANKS FOR UNDERSTANDING

using System.Text;
using Views.Menu.UnitsPopups;

namespace Views.Menu.UnitConverters;

public partial class UnitConvertersPageViewModel
{
    private (string AppConfigPropertyName, int AppConfigPropertyValue) AppConfigUnitPropertiesMap(EquationArea equationArea)
    {
        var ordinalName = equationArea == EquationArea.Top ? "First" : "Second";
        var appConfigPropertyName = $"{ordinalName}{PassedUnit.Name}";
        (string, int) appConfigUnitPropertiesMap = (appConfigPropertyName,
            (int)_appConfigService.GetType().GetProperty(appConfigPropertyName).GetValue(_appConfigService));

        return appConfigUnitPropertiesMap;
    }

    private void ChangeSelectedEquation(EquationArea equationArea)
    {
        // Change the selected equation (top, or bottom) and proceed to append
        // tapped digit(s), or decimal point, or invoke delete to the selected equation
        _equationArea = equationArea;
        (FirstEquationTxtColor, SecondEquationTxtColor) = equationArea == EquationArea.Top
            ? ("FF9501".ToColor(), (Color)Application.Current.Resources[AppColorResources.AdaptiveLabelColor])
            : ((Color)Application.Current.Resources[AppColorResources.AdaptiveLabelColor], "FF9501".ToColor());

        if (PassedUnit == typeof(NumberSystemUnit))
            ManipulateButtonsInteractivity((NumberSystemUnit)GetAppConfigUnitPropertyValue(equationArea));
    }

    private void ChangeSelectedUnit(object _, Tuple<EquationArea, int> args)
    {
        // Gets the selected unit from UnitsPopup via MessagingCenter then proceed with
        // the changes appropriately
        var unitId = args.Item2;

        if (args.Item1 == EquationArea.Top)
        {
            FirstEquationSIUnit = _unitConverterService.GetSIUnitById(unitId, PassedUnit);
            FirstEquationUnitName = _unitConverterService.GetUnitNameById(unitId, PassedUnit);

            _bottomEquationSb.Clear();
            _bottomEquationSb.Append(ConvertUnits(_topEquationSb));
            SecondEquation = _bottomEquationSb.ToString();
        }
        else
        {
            SecondEquationSIUnit = _unitConverterService.GetSIUnitById(unitId, PassedUnit);
            SecondEquationUnitName = _unitConverterService.GetUnitNameById(unitId, PassedUnit);

            _topEquationSb.Clear();
            _topEquationSb.Append(ConvertUnits(_bottomEquationSb));
            FirstEquation = _topEquationSb.ToString();
        }

        if (PassedUnit == typeof(NumberSystemUnit))
            ManipulateButtonsInteractivity((NumberSystemUnit)unitId);
    }

    private Task ChangeUnit(EquationArea equationArea)
    {
        // Opens the UnitPopup to change the unit
        ChangeSelectedEquation(equationArea);
        return NavigationService.OpenPopupAsync<UnitsPopup>(PassedUnit,
            equationArea,
            AppConfigUnitPropertiesMap(equationArea).AppConfigPropertyName,
            GetAppConfigUnitPropertyValue(equationArea));
    }

    private async Task<string> ConvertUnits(StringBuilder equationStringBuilder)
    {
        // Converts top, and bottom equations to their right values
        if (equationStringBuilder.Length == 0)
            return "0";

        var topEquationEnum = GetEquationUnitEnum(EquationArea.Top);
        var bottomEquationEnum = GetEquationUnitEnum(EquationArea.Bottom);

        string convertedUnit;
        if (PassedUnit == typeof(CurrencyConverterUnit))
        {
            var currencyConversionData = _equationArea switch
            {
                EquationArea.Top => _unitConverterService.ConvertUnits(equationStringBuilder, topEquationEnum, bottomEquationEnum),
                EquationArea.Bottom => _unitConverterService.ConvertUnits(equationStringBuilder, bottomEquationEnum, topEquationEnum),
                _ => throw new InvalidEnumArgumentException($"Unhandled value: {_equationArea}")
            };
            var awaitedCurrencyConversionData = await currencyConversionData;
            var slicedCurrencyConversionData = awaitedCurrencyConversionData.Split(' ');

            CurrencyConversionLastUpdate = DateTime.Parse(slicedCurrencyConversionData[0]);
            convertedUnit = slicedCurrencyConversionData[1];
            IsCurrencyConversionDateVisible = CurrencyConversionLastUpdate != default;
        }
        else
            convertedUnit = _equationArea switch
            {
                EquationArea.Top => await _unitConverterService.ConvertUnits(equationStringBuilder, topEquationEnum, bottomEquationEnum),
                EquationArea.Bottom => await _unitConverterService.ConvertUnits(equationStringBuilder, bottomEquationEnum, topEquationEnum),
                _ => throw new InvalidEnumArgumentException($"Unhandled value: {_equationArea}")
            };


        return PassedUnit == typeof(NumberSystemUnit) ? convertedUnit : _unitConverterService.EquationPrettier(convertedUnit);
    }

    private int GetAppConfigUnitPropertyValue(EquationArea equationArea)
    {
        // Gets the correct app config property value
        // If the current converter is AreaConverter;
        // Return FirstAreaUnit's value from app config if the top equation is selected
        // Return SecondAreaUnit's value from app config if the bottom equation is selected
        var appConfigUnit = AppConfigUnitPropertiesMap(equationArea).AppConfigPropertyValue;
        return appConfigUnit;
    }

    private Enum GetEquationUnitEnum(EquationArea equationArea) //here
    {
        // Gets the correct UnitsNet enum to perform the correct conversion
        // To perform the correct conversion, UnitsNet requires fromUnit and toUnit parameters
        var appConfigPropertyValue = AppConfigUnitPropertiesMap(equationArea).AppConfigPropertyValue;
        var isParsedSuccesfully = Enum.TryParse(PassedUnit, appConfigPropertyValue.ToString(), out var parsedEnum);

        return isParsedSuccesfully
            ? parsedEnum as Enum
            : throw new ArgumentException($"Unhandled value: {PassedUnit}");
    }
}