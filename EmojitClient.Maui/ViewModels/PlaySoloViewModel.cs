using EmojitClient.Maui.Data;
using EmojitClient.Maui.Data.Entities;
using EmojitClient.Maui.Enums;
using EmojitClient.Maui.Framework;
using EmojitClient.Maui.Framework.Base;
using EmojitClient.Maui.Framework.Navigation;
using EmojitClient.Maui.Framework.Services;
using EmojitClient.Maui.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;

namespace EmojitClient.Maui.ViewModels;

/// <summary>
/// Handles solo game logic: rounds, symbols, and timer using SpotItDesign + DB assets.
/// </summary>
public partial class PlaySoloViewModel : ViewModelBase
{
    public ObservableCollection<string> EmojiList { get; } = ["🔥", "💎", "🎉", "🍀"];  //, "🐱", "🌈", "🍩", "🚀", "❤️", "⭐", "😋", "😎", "🎅"];

    public ICommand EmojiTappedCommand => new Command<string>(emoji => Debug.WriteLine($"[Tapped] {emoji}"));

    private readonly GameDataRepository dataRepo;
    private readonly ISoundService soundService;
    private EmojItDesign? design;
    private EmojItManager? manager;

    private readonly System.Timers.Timer timer;

    private int currentRound;
    private int score;
    private int timeLeft;
    private bool isGameOver;

    private int cardA;
    private int cardB;
    private int commonSymbol;

    private List<Symbol> themeSymbols = [];
    private DifficultyLevel difficulty;
    private const int themeId = 2;

    public ObservableCollection<EmojItSymbol> PlayerSymbols { get; } = [];
    public ObservableCollection<EmojItSymbol> StackSymbols { get; } = [];

    public int CurrentRound { get => currentRound; set => SetProperty(ref currentRound, value); }
    public int Score { get => score; set => SetProperty(ref score, value); }
    public int TimeLeft { get => timeLeft; set => SetProperty(ref timeLeft, value); }

    public ICommand SelectSymbolCommand { get; }
    public ICommand EndGameCommand { get; }

    public event Action? RoundTransitionRequested;
    public event Action<EmojItSymbol>? CorrectSymbolHit;
    public event Action? TimeoutShakeRequested;

    public PlaySoloViewModel(NavigationManager navigation, GameDataRepository repo, ISoundService soundService) : base(navigation)
    {
        this.soundService = soundService;
        dataRepo = repo;
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += Timer_Elapsed;

        SelectSymbolCommand = new Command<EmojItSymbol>(OnSymbolSelected);
        EndGameCommand = new Command(async () => await EndGameAsync());
    }

    public override async Task OnNavigatedToAsync(object? parameter)
    {
        if (parameter is DifficultyLevel diff)
            difficulty = diff;

        await InitializeGameAsync();
    }

    private async Task InitializeGameAsync()
    {
        Score = 0;
        CurrentRound = 1;
        TimeLeft = 10;
        isGameOver = false;

        // Adjust design based on difficulty (prime orders)
        int n = difficulty switch
        {
            DifficultyLevel.Easy => 3,    // 13 symbols per card
            DifficultyLevel.Normal => 5,  // 31 symbols per card
            DifficultyLevel.Hard => 7,    // 57 symbols per card
            _ => 3
        };

        // Créer la conception mathématique
        design = EmojItDesign.Create(n);
        manager = new EmojItManager(design);

        // Charger les symboles du thème
        themeSymbols = await dataRepo.GetSymbolsByThemeAsync(themeId, design.SymbolCount);

        GenerateNewRound();
        timer.Start();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (isGameOver) return;

        TimeLeft--;
        if (TimeLeft <= 0)
        {
            TimeoutShakeRequested?.Invoke();
            OnRoundTimeout();
        }
    }

    private void OnRoundTimeout()
    {
        if (isGameOver) return;
        soundService.PlaySfx("error");

        if (CurrentRound >= 10)
        {
            MainThread.BeginInvokeOnMainThread(async () => await EndGameAsync());
            return;
        }

        CurrentRound++;
        GenerateNewRound();
        TimeLeft = 10;
        RoundTransitionRequested?.Invoke();
    }

    private void OnSymbolSelected(EmojItSymbol selected)
    {
        if (isGameOver) return;

        if (selected.Id == commonSymbol)
        {
            soundService.PlaySfx("success");
            CorrectSymbolHit?.Invoke(selected);
            Score++;

            if (CurrentRound >= 10)
            {
                _ = EndGameAsync();
                return;
            }

            CurrentRound++;
            GenerateNewRound();
            TimeLeft = 10;
            RoundTransitionRequested?.Invoke();
        }
        else
        {
            soundService.PlaySfx("error");

            if (CurrentRound >= 10)
            {
                _ = EndGameAsync();
                return;
            }

            CurrentRound++;
            GenerateNewRound();
            TimeLeft = 10;
            RoundTransitionRequested?.Invoke();
        }
    }

    private void GenerateNewRound()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PlayerSymbols.Clear();
            StackSymbols.Clear();

            manager!.NextRandomPair(out cardA, out cardB, out commonSymbol);

            IReadOnlyList<int> cardAIds = design!.GetCard(cardA);
            IReadOnlyList<int> cardBIds = design!.GetCard(cardB);

            foreach (int id in cardAIds)
            {
                PlayerSymbols.Add(MapSymbolToUi(id));
            }

            foreach (int id in cardBIds)
            {
                StackSymbols.Add(MapSymbolToUi(id));
            }

            EmojiList.Clear();
            foreach (string c in new[] { "🔥", "💎", "🎉", "🍀", "🐱", "🌈", "🍩", "🚀" }) //, "❤️", "⭐", "😋", "😎", "🎅" })
            {
                EmojiList.Add(c);
            }
        });
    }

    private EmojItSymbol MapSymbolToUi(int symbolId)
    {
        Symbol symbol = themeSymbols.Count > 0 && themeSymbols.Count > symbolId
            ? themeSymbols[symbolId % themeSymbols.Count]
            : new Symbol
            {
                Id = -1,
                Label = $"Symbol {symbolId}",
                Emoji = "❌",
                ImageBlob = [],
                MimeType = "null",
                ThemeId = 1
            };

        return new EmojItSymbol
        {
            Id = symbolId,
            Label = symbol.Emoji,
            ImageBlob = symbol.ImageBlob,
            MimeType = symbol.MimeType ?? "image/png",
            Image = ImageSource.FromStream(() => new MemoryStream(symbol.ImageBlob))
        };
    }


    private async Task EndGameAsync()
    {
        isGameOver = true;
        timer.Stop();

        var stats = new
        {
            Score,
            Rounds = CurrentRound,
            Difficulty = difficulty
        };

        await Navigation.NavigateToAsync<StatsSoloViewModel>(stats);
        soundService.StopBgm();
        soundService.PlayBgm("victory");
    }
}