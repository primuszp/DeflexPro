# DeflexPro

DeflexPro is a Windows desktop application for importing, inspecting, visualizing, and exporting Falling Weight Deflectometer (FWD) measurement data.

## Implemented Features

- Imports KUAB and Primax `.fwd` measurement files.
- Lists measurement stations, drops, sensor offsets, load values, timestamps, and air and pavement temperatures.
- Calculates and displays the D0, SCI, BDI, and BCI deflection-basin indicators.
- Fits and plots a deflection-basin curve for each selected drop, including the fit RMSE.
- Supports selecting stations and individual drops for focused inspection.
- Exports loaded measurement summaries to CSV.
- Provides a pavement-layer structure editor with configurable thickness and modulus bounds.
- Copies layer structures to selected stations or a distance range.
- Assigns stations to named groups.
- Runs native OpenPave layered-elastic forward calculations and FWD backcalculation.
- Provides a validated .NET wrapper for native OpenPave heat-transfer calculations.
- Exports generated layer-modulus result rows to CSV.
- Uses a dark, high-contrast WPF interface.
- Provides English and Hungarian user interfaces, selected automatically from the Windows display language. English is used for all other languages.

## Technology

- .NET 10 for Windows
- WPF
- MVVM architecture
- OxyPlot.Wpf 2.2.0

## Requirements

- Windows 10 or Windows 11
- .NET 10 SDK to build from source
- .NET 10 Desktop Runtime to run a published framework-dependent build

## Build and Run

```powershell
dotnet restore
dotnet run --project DeflexPro/DeflexPro.csproj
```

Create a release build:

```powershell
dotnet build DeflexPro.sln --configuration Release
```

## Project Structure

```text
DeflexPro/
├── Controls/       Reusable WPF controls
├── Converters/     WPF value converters
├── Images/         Application image assets
├── Localization/   English and Hungarian UI resources
├── Model/          FWD models, file readers, and curve-fitting logic
├── View/           WPF views
└── ViewModel/      MVVM presentation logic
```

## OpenPave Native Wrapper

The managed API is under `DeflexPro/OpenPave`:

- `OpenPaveService.Calculate` performs layered-elastic forward calculation.
- `OpenPaveService.Backcalculate` performs modulus backcalculation and calculates fitted-basin RMSE.
- `OpenPaveService.CreateHeatModel` creates a native FEM heat-transfer model for stepping and interpolation.

OpenPave mechanical units are mm, MPa, and N. The application's legacy FWD model stores deflections in micrometers and peak force in 0.01 kN; both are explicitly converted before backcalculation.
