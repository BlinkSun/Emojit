# 🧩 Procédure d’intégration d’une nouvelle catégorie d’emojis dans SpotIt (ex. *Food & Drink*)

## 🎯 Objectif
Importer automatiquement une nouvelle catégorie d’emojis (par ex. *Food & Drink*) depuis le dépôt officiel **Google Noto Emoji**, en créant des enregistrements dans la base SQLite `spotit.db`.

---

## 📁 Prérequis

1. **Fichiers nécessaires:**
   - Le fichier JSON officiel du dépôt Noto Emoji :  
     [`emoji_17_0_ordering.json`](https://github.com/googlefonts/noto-emoji/blob/main/emoji_ordering/emoji_17_0_ordering.json)
   - Le dossier contenant les fichiers PNG extraits du dépôt :  
     (ex. `noto-emoji/png/128/`)
   - Le fichier SQLite `spotit.db`
   - Le fichier de mapping texte (sera généré plus bas).

2. **Structure de la base de données:**
   ```sql
   CREATE TABLE Symbol (
       Id        INTEGER PRIMARY KEY AUTOINCREMENT,
       ThemeId   INTEGER NOT NULL,
       Label     TEXT,
       Emoji     TEXT,
       ImageBlob BLOB,
       MimeType  TEXT,
       FOREIGN KEY (ThemeId) REFERENCES Theme (Id)
   );
   ```

3. **Préparation:**
   - Place **le fichier `.db`**, **le JSON**, **le script C# console**, et **les PNG** dans le **même dossier**.
   - Chaque catégorie (Animals, Food, etc.) doit avoir un **ThemeId distinct** dans la table `Theme`.

---

## 🧠 Étape 1 — Extraire la liste des emojis de la catégorie

### 1.1 Identifier la catégorie dans le JSON
Chaque groupe est structuré comme ceci :
```json
{
  "group": "Food and drink",
  "emoji": [
    {
      "base": [127828],
      "shortcodes": [":hamburger:"],
      "animated": false
    },
    ...
  ]
}
```

- `group` → nom de la catégorie (`Food and drink`)
- `base` → code Unicode en **décimal**
- `shortcodes` → nom de l’emoji entre `: :` (`:hamburger:`)

### 1.2 Convertir le code en nom de fichier
Chaque valeur décimale dans `"base"` doit être convertie en **hexadécimal** :
```
127828 (decimal) → 1F354 (hex)
```
Le fichier correspondant dans le repo PNG s’appelle :
```
emoji_u1f354.png
```

### 1.3 Construire le fichier de mapping
Pour chaque emoji de la catégorie, crée un fichier texte :
```
emoji_u1f354.png : hamburger
emoji_u1f355.png : pizza
emoji_u1f356.png : meat-on-bone
...
```
## ⚡ Génération du fichier de mapping avec PowerShell

Pour générer rapidement le fichier de correspondance entre les fichiers PNG et les noms d’emojis à partir du JSON officiel de Google Noto Emoji, tu peux utiliser ce script PowerShell.

### 🧩 Script PowerShell — `Generate-Mapping.ps1`

```powershell
# ============================
# Génération du fichier mapping Emoji
# ============================
# Prérequis :
# - emoji_17_0_ordering.json (du repo Noto Emoji)
# - Catégorie cible (ex: "Food and drink")
# ============================

param(
    [string]$JsonPath = ".\emoji_17_0_ordering.json",
    [string]$Category = "Food and drink",
    [string]$OutputFile = ".\emoji_food_mapping.txt"
)

if (-not (Test-Path $JsonPath)) {
    Write-Host "❌ Fichier JSON introuvable : $JsonPath"
    exit
}

# Lecture du JSON
Write-Host "📖 Lecture du fichier JSON..."
$jsonData = Get-Content $JsonPath -Raw | ConvertFrom-Json

# Trouver la catégorie demandée
$categoryGroup = $jsonData | Where-Object { $_.group -eq $Category }

if (-not $categoryGroup) {
    Write-Host "❌ Catégorie '$Category' non trouvée dans le JSON."
    exit
}

Write-Host "✅ Catégorie trouvée : $($categoryGroup.group)"
$lines = @()

# Parcourir les emojis de cette catégorie
foreach ($emoji in $categoryGroup.emoji) {
    if ($emoji.base.Count -eq 0 -or $emoji.shortcodes.Count -eq 0) { continue }

    # Conversion du code décimal en hex
    $code = "{0:x}" -f $emoji.base[0]
    $fileName = "emoji_u$code.png"

    # Nettoyer le shortcode (ex: :hamburger: -> hamburger)
    $name = $emoji.shortcodes[0].Trim(":")
    
    # Ajouter la ligne
    $lines += "$fileName : $name"
}

# Sauvegarde du fichier
$lines | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "✅ Fichier mapping généré : $OutputFile"
Write-Host "💾 Contient $($lines.Count) lignes."
```

### ⚙️ Exemple d’exécution

Depuis le dossier contenant `emoji_17_0_ordering.json` :

```powershell
.\Generate-Mapping.ps1 -Category "Food and drink"
```

Résultat :
```
✅ Fichier mapping généré : .\emoji_food_mapping.txt
💾 Contient 89 lignes.
```

### 📄 Exemple de sortie

```
emoji_u1f354.png : hamburger
emoji_u1f355.png : pizza
emoji_u1f356.png : meat-on-bone
```

💡 Pour une autre catégorie :
```powershell
.\Generate-Mapping.ps1 -Category "Animals & Nature"
```

---

## 🐍 Étape 2 — (Facultatif) Ajouter les traductions
Si tu veux afficher le nom en français, tu peux enrichir le fichier comme suit :
```
emoji_u1f354.png : hamburger : Hamburger
emoji_u1f355.png : pizza : Pizza
emoji_u1f356.png : meat-on-bone : Cuisse de viande
```

- Le premier champ → nom du fichier
- Le deuxième → nom anglais (`emoji`)
- Le troisième → traduction française (`Label`)

---

## 💾 Étape 3 — Importer dans SQLite

### 3.1 Configuration du projet C#
Crée une **application console .NET 8** et ajoute le package suivant :
```bash
dotnet add package Microsoft.Data.Sqlite
```

### 3.2 Script C# d’insertion
```csharp
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace EmojiImporter
{
    class Program
    {
        static void Main()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dbPath = Path.Combine(baseDir, "spotit.db");
                string mappingFile = Path.Combine(baseDir, "emoji_food_mapping_fr.txt");
                int themeId = 2; // Exemple : 1 = Animaux, 2 = Nourriture

                if (!File.Exists(dbPath) || !File.Exists(mappingFile))
                {
                    Console.WriteLine("❌ Fichier manquant (DB ou mapping)");
                    return;
                }

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var lines = File.ReadAllLines(mappingFile);
                    int inserted = 0;

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(" : ", StringSplitOptions.TrimEntries);
                        if (parts.Length < 3) continue;

                        string fileName = parts[0];
                        string emojiName = parts[1];
                        string labelFr = parts[2];

                        string imgPath = Path.Combine(baseDir, fileName);
                        if (!File.Exists(imgPath))
                        {
                            Console.WriteLine($"⚠️ Image non trouvée : {fileName}");
                            continue;
                        }

                        byte[] blob = File.ReadAllBytes(imgPath);

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Symbol (ThemeId, Label, Emoji, ImageBlob, MimeType)
                                VALUES ($ThemeId, $Label, $Emoji, $ImageBlob, $MimeType)";
                            cmd.Parameters.AddWithValue("$ThemeId", themeId);
                            cmd.Parameters.AddWithValue("$Label", labelFr);
                            cmd.Parameters.AddWithValue("$Emoji", emojiName);
                            cmd.Parameters.AddWithValue("$ImageBlob", blob);
                            cmd.Parameters.AddWithValue("$MimeType", "image/png");
                            cmd.ExecuteNonQuery();
                        }

                        inserted++;
                    }

                    Console.WriteLine($"✅ Importation terminée : {inserted} symboles ajoutés à la catégorie {themeId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("💥 Erreur : " + ex.Message);
            }
        }
    }
}
```

---

## 🧩 Étape 4 — Validation
1. Exécute ton exécutable dans le même dossier que :
   - `spotit.db`
   - `emoji_food_mapping_fr.txt`
   - Les fichiers `.png`
2. Vérifie la table :
   ```sql
   SELECT Label, Emoji FROM Symbol WHERE ThemeId = 2;
   ```
   ✅ Chaque ligne doit correspondre à un emoji de *Food & Drink*.

---

## 🚀 Résumé de la méthode

| Étape | Action | Résultat |
|-------|---------|-----------|
| 1 | Extraire la catégorie “Food and Drink” du JSON | Liste d’emojis avec leurs codes |
| 2 | Convertir les codes décimaux en hexadécimal | Correspondance avec les fichiers PNG |
| 3 | Créer un fichier texte mapping | `emoji_uXXXX.png : shortcode : traduction` |
| 4 | Exécuter le script C# | Import automatique dans SQLite |
| 5 | Vérifier dans `spotit.db` | Nouvelles entrées visibles dans `Symbol` |

---

## 🧠 Notes techniques
- `base` dans le JSON = code Unicode en **décimal**
- Conversion en hex = `int.ToString("x")`
- Fichier image = `emoji_uXXXX.png`
- `shortcode` → valeur du champ `Emoji`
- Traduction facultative → champ `Label`
- `ThemeId` doit correspondre à la catégorie (définie dans la table `Theme`)

---

## ✅ Exemple de résultat final dans la base
| Id | ThemeId | Label (FR) | Emoji (EN) | MimeType | ImageBlob |
|----|----------|-------------|-------------|-----------|------------|
| 1 | 1 | Vache | cow-face | image/png | (blob) |
| 2 | 2 | Pizza | pizza | image/png | (blob) |
| 3 | 2 | Hamburger | hamburger | image/png | (blob) |

---

> ✨ En suivant ce guide, tu peux ajouter **n’importe quelle catégorie** d’emojis du dépôt Google Noto Emoji (People, Animals, Food, etc.) directement à ta base SpotIt, sans dépendance externe ni scripts complexes.
