//READ ME:
// THE OOP PROJECT THAT I DEMONSTRATED IS FROM A COLLEAGUE
// HE ONLY ALLOWS TO USE THE FILES THAT ARE DEMONSTRATED THE VIDEO
// SO THERE WILL BE LOT OF SQUIGGLY LINES WHEN LAUNCHED ON VISUAL STUDIO
// THANKS FOR UNDERSTANDING

using Views.Menu.Age.Calendar;

namespace Views.Menu.Age;

public partial class AgePageViewModel
{
    private void AdaptElementColors(object _, AppThemeChangedEventArgs __)
    {
        FavoriteBtnTextColor = FavoriteBtnTextColor == FavoriteColor
            ? FavoriteColor
            : (Color)Application.Current.Resources[AppColorResources.CalculatorButtonTextColor];
    }

    private void AddToCalendar(DateTime dateToAdd)
    {
        _addEventToCalendar.AddToCalendar(DateOfBirth);
    }

    private void ChangeFavoriteBtnState(bool isAddedAsFavorite)
    {
        FavoriteBtnTextColor = isAddedAsFavorite
            ? FavoriteColor
            : (Color)Application.Current.Resources[AppColorResources.CalculatorButtonTextColor];
    }

    public override void Init()
    {
        ChangeFavoriteBtnState(_appConfigService.ManipulateFavoriteCalculators(CalculatorTypes.Age, true));
        CurrentDate = DateTime.Now;
        var startDate = DateTime.UnixEpoch;
        DateOfBirth = startDate.AddDays(_randomizer.Next((DateTime.Today - startDate).Days));
        SolveAge(CurrentDate, DateOfBirth);

        //CurrentDate = DateTime.Now;
        //DateOfBirth = new DateTime(2021, 10, 22);
        //SolveAge(CurrentDate, DateOfBirth);
    }

    private void ManipulateFavorite()
    {
        var isAddedAsFavorite = _appConfigService.ManipulateFavoriteCalculators(CalculatorTypes.Age);
        ChangeFavoriteBtnState(isAddedAsFavorite);
    }

    public override void OnAppearing()
    {
        MessagingCenter.Subscribe<object, (string, DateTime)>(this,
            AppMessages.AgeCalendarChanged,
            async (_, args) => await SolveAge(args));
    }

    public override void OnDisappearing()
    {
        MessagingCenter.Unsubscribe<object, (string, DateTime)>(this, AppMessages.AgeCalendarChanged);
    }

    private Task OpenCalendar(int arg)
    {
        var calendarHeader = arg == 0 ? "Current Date" : "Date of Birth";
        var passedDate = arg == 0 ? CurrentDate : DateOfBirth;
        return NavigationService.OpenPopupAsync<CalendarPopup>(calendarHeader, passedDate);
    }

    private void SolveAge(DateTime currentDate, DateTime dateOfBirth)
    {
        var allDaysAlive = Enumerable.Range(1, (currentDate - dateOfBirth).Days)
            .Select(_ => dateOfBirth.AddDays(_))
            .ToList();
        CurrentAgeYears = allDaysAlive.Count(_ => _.Month == dateOfBirth.Month && _.Day == dateOfBirth.Day);
        MonthsBreakdown = $"{allDaysAlive.Skip(1).Count(_ => _.Day == dateOfBirth.Day):n0}";

        var allDaysCount = allDaysAlive.Count;
        WeeksBreakdown = $"{allDaysCount / 7:n0}";
        DaysBreakdown = $"{allDaysCount:n0}";
        HoursBreakdown = $"{allDaysCount * 24:n0}";
        MinutesBreakdown = $"{allDaysCount * 1440:n0}";
        SecondsBreakdown = $"{allDaysCount * 86400:n0}";

        var bdayThisYear = new DateTime(currentDate.Year, dateOfBirth.Month, dateOfBirth.Day);
        NextBirthdayDayDayOfWeek = bdayThisYear.AddYears(1).DayOfWeek;

        var minifiedCurrentDate = new DateTime(1, currentDate.Month, currentDate.Day);
        var minifiedDateOfBirth = new DateTime(1, dateOfBirth.Month, dateOfBirth.Day);
        var bufferYearForExtraAge = minifiedCurrentDate < minifiedDateOfBirth ? 1 : 0;

        var datesForExtraAge = new List<DateTime>();
        for (var date = bdayThisYear.AddDays(1); date <= currentDate.AddYears(bufferYearForExtraAge); date = date.AddDays(1))
            datesForExtraAge.Add(date);

        ExtraMonths = datesForExtraAge.Count(_ => _.Day == dateOfBirth.Day);
        var lastExtraWholeMonth = datesForExtraAge.LastOrDefault(_ => _.Day == dateOfBirth.Day);
        //ExtraDays = currentDate.DayOfYear - lastExtraWholeMonth.DayOfYear;
        //ExtraDays = datesForExtraAge.Count(_ => _ > lastExtraWholeMonth &&  _.Day > lastExtraWholeMonth.Day);
        ExtraDays = datesForExtraAge.Count(_ => _ > lastExtraWholeMonth);

        var bufferYearForNextBday = minifiedCurrentDate < minifiedDateOfBirth ? 0 : 1;
        var nextBday = new List<DateTime>();
        for (var date = currentDate; date <= bdayThisYear.AddYears(bufferYearForNextBday); date = date.AddDays(1))
            nextBday.Add(date);

        NextBirthDayMonths = nextBday.Count(_ => _.Day == dateOfBirth.Day);
        var nextBdayLastWholeMonth = nextBday.LastOrDefault(_ => _.Day == currentDate.Day);
        NextBirthdayDays = dateOfBirth.DayOfYear - nextBdayLastWholeMonth.DayOfYear;
    }

    private async Task SolveAge((string calendarHeader, DateTime passedDate) args)
    {
        var calendarHeader = args.calendarHeader;
        var passedDate = args.passedDate;
        if (calendarHeader == "Date of Birth")
            if (passedDate > CurrentDate)
            {
                await NavigationService.ClosePopupAsync();
                await NavigationService.ShowAlertPopupAsync("Invalid date",
                    "The birth date is more than the current date. Please try to enter a date that is lower than the current date",
                    "OK");
                return;
            }

        if (calendarHeader == "Current Date")
            CurrentDate = passedDate;
        else
            DateOfBirth = passedDate;

        SolveAge(CurrentDate, DateOfBirth);
    }
}