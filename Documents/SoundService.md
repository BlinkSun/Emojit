# 🎧 SoundService (MAUI)

Service audio unifié pour la gestion de la musique de fond (BGM) et des effets sonores (SFX)  
✅ Compatible Android et Windows  
✅ Injection via **Dependency Injection (.NET MAUI)**  
✅ Persistance automatique (préférences locales)  
✅ Aucune écriture de fichiers temporaires  
✅ Sans latence et sans fade  

---

## 📂 Structure des ressources

```
Resources/
 ├── Raw/
 │    ├── click.wav
 │    ├── success.wav
 │    ├── error.wav
 │    ├── menu.mp3
 │    ├── battle.mp3
 │    └── victory.mp3
```

> 💡 Les fichiers doivent être marqués comme **Resource** dans le projet (.csproj).

```xml
<ItemGroup>
  <MauiAsset Include="Resources\Raw\**" LogicalName="Resources/Raw/%(Filename)%(Extension)" />
</ItemGroup>
```

---

## ⚙️ Enregistrement du service

Dans `MauiProgram.cs` :

```csharp
using SpotIt.Maui.Framework.Services;

builder.Services.AddSingleton<ISoundService, SoundService>();
```

---

## 🚀 Initialisation (App.cs)

```csharp
using SpotIt.Maui.Framework.Services;

public partial class App : Application
{
    private readonly ISoundService soundService;

    public App(MainMenuPage mainPage, ISoundService soundService)
    {
        InitializeComponent();
        this.soundService = soundService;
        _ = InitializeAudioAsync();
    }

    private async Task InitializeAudioAsync()
    {
        await soundService.InitializeAsync();
        soundService.PlayBgm("menu");
    }
}
```

---

## 🎮 Utilisation typique

### Jouer la musique du menu
```csharp
soundService.PlayBgm("menu");
```

### Jouer un effet sonore
```csharp
soundService.PlaySfx("click");
```

### Démarrer la musique du combat
```csharp
soundService.PlayBgm("battle");
```

### Lors d'une victoire
```csharp
soundService.StopBgm();
soundService.PlaySfx("success");
soundService.PlayBgm("victory");
```

---

## 🧩 API du service

| Méthode / Propriété        | Description |
|----------------------------|-------------|
| `InitializeAsync()`        | Charge en mémoire tous les sons et musiques déclarés |
| `PlayBgm(string name)`     | Joue une musique de fond en boucle |
| `StopBgm()`                | Arrête la musique courante |
| `PlaySfx(string name)`     | Joue un effet sonore instantané |
| `ToggleMusic(bool enabled)`| Active/désactive la musique (persistant) |
| `ToggleSfx(bool enabled)`  | Active/désactive les SFX (persistant) |
| `MuteAll()` / `RestoreVolumes()` | Permet de couper puis restaurer les volumes |
| `BgmVolume` / `SfxVolume`  | Contrôle séparé des volumes globaux |
| `Dispose()`                | Libère toutes les ressources audio |

---

## 💾 Persistance automatique

Les préférences suivantes sont stockées avec `Preferences` :

| Clé | Valeur par défaut | Description |
|-----|--------------------|-------------|
| `SoundService.MusicEnabled` | `true` | Active ou non la musique |
| `SoundService.SfxEnabled`   | `true` | Active ou non les effets sonores |

---

## 🧠 Exemple d’injection dans une page

```csharp
public partial class MainPage : ContentPage
{
    private readonly ISoundService soundService;

    public MainPage(ISoundService soundService)
    {
        InitializeComponent();
        this.soundService = soundService;
    }

    private void OnButtonClick(object sender, EventArgs e)
    {
        soundService.PlaySfx("click");
    }
}
```

---

## ✅ Résumé des avantages

- 🔁 Aucune duplication de code entre plateformes
- 🎚️ Volume séparé pour BGM et SFX
- 💾 Persistance automatique
- 💡 DI-friendly (testable, remplaçable)
- ⚡ Préchargement en mémoire = pas de latence

---
© 2025 — SpotIt.Maui Audio Framework by BlinkSun