# DeflexPro

**FWD (Falling Weight Deflectometer) adatelemző és visszaszámító alkalmazás**  
**FWD (Falling Weight Deflectometer) data analysis and back-calculation application**

---

## 🇭🇺 Magyar

### Leírás

A **DeflexPro** egy asztali WPF alkalmazás útburkolati szerkezetek FWD mérési adatainak megjelenítéséhez, elemzéséhez és a rétegmodulus visszaszámításához.

### Főbb funkciók

- **FWD fájlbeolvasás** – KUAB és Primax `.fwd` formátum támogatása
- **Behajlási medence megjelenítés** – ejtésenkénti görbeillesztés OxyPlot diagramon
- **Behajlási indexek** – D₀, SCI (Surface Curvature Index), BDI (Base Damage Index), BCI (Base Curvature Index) automatikus számítása
- **Szenzorértékek** – összes mért szenzor behajlási értéke μm-ben
- **Visszaszámítás modul** *(fejlesztés alatt)* – szelvényenkénti egyedi vagy csoportos pályaszerkezet-rétegrend megadása, modulusvisszaszámítás
- **Sötét téma** – professzionális, kontrasztos felület

### Technológiák

| Komponens | Verzió |
|-----------|--------|
| .NET | 10.0-windows |
| WPF | MVVM mintázat |
| OxyPlot.Wpf | 2.2.0 |
| C# | 12 |

### Rendszerkövetelmények

- Windows 10 / 11 (x64)
- .NET 10 Runtime

### Telepítés / futtatás

```bash
git clone https://github.com/<felhasználó>/DeflexPro.git
cd DeflexPro
dotnet run --project DeflexPro/DeflexPro.csproj
```

### Projekt struktúra

```
DeflexPro/
├── Model/          – FWD adatmodellek, fájlolvasók, FittBasin illesztés
├── ViewModel/      – MVVM ViewModelek (fő, plot, visszaszámítás)
├── View/           – WPF XAML nézetek
├── Controls/       – Újrafelhasználható vezérlők (NavigationPane)
├── Converters/     – IValueConverter implementációk
└── Images/         – UI ikonok
```

---

## 🇬🇧 English

### Description

**DeflexPro** is a desktop WPF application for visualising, analysing and back-calculating pavement layer moduli from FWD measurement data.

### Key Features

- **FWD file import** – supports KUAB and Primax `.fwd` formats
- **Deflection basin visualisation** – per-drop curve fitting displayed on an OxyPlot chart
- **Deflection indices** – automatic calculation of D₀, SCI (Surface Curvature Index), BDI (Base Damage Index) and BCI (Base Curvature Index)
- **Sensor readings** – all measured sensor deflections in µm
- **Back-calculation module** *(in development)* – per-section individual or grouped pavement layer structure definition and modulus back-calculation
- **Dark theme** – professional, high-contrast UI

### Technology Stack

| Component | Version |
|-----------|---------|
| .NET | 10.0-windows |
| WPF | MVVM pattern |
| OxyPlot.Wpf | 2.2.0 |
| C# | 12 |

### System Requirements

- Windows 10 / 11 (x64)
- .NET 10 Runtime

### Installation / Running

```bash
git clone https://github.com/<user>/DeflexPro.git
cd DeflexPro
dotnet run --project DeflexPro/DeflexPro.csproj
```

### Project Structure

```
DeflexPro/
├── Model/          – FWD data models, file readers, FittBasin curve fitting
├── ViewModel/      – MVVM ViewModels (main, plot, back-calculation)
├── View/           – WPF XAML views
├── Controls/       – Reusable controls (NavigationPane)
├── Converters/     – IValueConverter implementations
└── Images/         – UI icons
```

### License

This project is licensed under the MIT License.
