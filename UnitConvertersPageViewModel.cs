//READ ME:
// THE OOP PROJECT THAT I DEMONSTRATED IS FROM A COLLEAGUE
// HE ONLY ALLOWS TO USE THE FILES THAT ARE DEMONSTRATED THE VIDEO
// SO THERE WILL BE LOT OF SQUIGGLY LINES WHEN LAUNCHED ON VISUAL STUDIO
// THANKS FOR UNDERSTANDING

using Services.UnitConverterServices;
using System.Text;

namespace Views.Menu.UnitConverters;

public partial class UnitConvertersPageViewModel : BaseViewModel
{
    private readonly AppConfigService _appConfigService;
    private readonly StringBuilder _bottomEquationSb;
    private char[] _digitKeys;
    private EquationArea _equationArea; // Refers to if the top equation will be changed or the bottom equation
    private readonly Dictionary<char, bool> _interactivityDict;
    private bool[] _interactivityValues;
    private readonly StringBuilder _topEquationSb;
    private readonly UnitConverterService _unitConverterService;

    public UnitConvertersPageViewModel(AppConfigService appConfigService,
        UnitConverterService unitConverterService)
    {
        _appConfigService = appConfigService;
        _bottomEquationSb = new StringBuilder();
        _interactivityDict = new Dictionary<char, bool>();
        _topEquationSb = new StringBuilder();
        _unitConverterService = unitConverterService;

        Application.Current.RequestedThemeChanged += AdaptElementColors;
        AllClearCmd = new Command(AllClear);
        AppendDecimalPointCmd = new Command(AppendDecimalPoint);
        ChangeUnitCmd = new AsyncSingleCommand<EquationArea>(ChangeUnit);
        ChangeSelectedEquationCmd = new Command<EquationArea>(ChangeSelectedEquation);
        DeleteCmd = new Command(Delete);
        DigitTappedCmd = new AsyncSingleCommand<char>(AppendDigitToEquation);
        ManipulateFavoriteCmd = new Command(ManipulateFavorite);
    }
}