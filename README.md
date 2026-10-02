# PVRSA Indicator

![PVRSA]([http://url/to/img.png](https://github.com/EddyT1717/PVRSA-I/blob/main/PVRSA-Indicator.png?raw=true))

Quantower C# overlay indicator implementing the PVRSA context pack:

- PVSRA vector candles (climax / rising volume classification)
- EMA stack (5/13/50/200/800) + 50-EMA cloud
- Floor pivots (P/R/S + M mids)
- YDay / LWeek high-low
- ADR / AWR / AMR / RD / RW range projections
- DST-aware session boxes, Psy Hi/Lo, DST table
- PVSRA-candle zones (unrecovered liquidity)

---

## 1. Install (deploy the release file)

> Skip to **Build** (section 2) if you're compiling the DLL yourself. Otherwise the release
> `PVRSAIndicator.dll` goes here:

1. Copy Folder `PVRSA` to Quantower's custom-indicator folder:
   `...\Quantower\Settings\Scripts\Indicators\`
2. Restart Quantower if open.
3. Add **PVRSAIndicator** to a price chart.

---

## 2. Build

Compile the indicator to a release. You do **not** need Visual Studio — a `.NET SDK` and the
Quantower API DLL are enough.

### 2.1 The Quantower API DLL

The build needs `TradingPlatform.BusinessLayer.dll` from a Quantower install. Quantower ships
portable (no install-location env var); the API DLL lives in a **versioned** subfolder. Find the
current one:

```powershell
Get-ChildItem -Path "C:\PATH\TO\Quantower" -Recurse -Filter "TradingPlatform.BusinessLayer.dll" | % { $_.FullName }
```

`PVRSAIndicator.csproj` references the DLL via `$(QuantowerDir)`, so the build command supplies
it pointing at the `bin` folder that contains `TradingPlatform.BusinessLayer.dll` (e.g.
`<quantower>\TradingPlatform\v1.147.4\bin`). If a Quantower update moves the DLL tree, update the
version subfolder of `QuantowerDir`.

### 2.2 Build

```bash
dotnet build PVRSAIndicator.csproj -c Release -p:QuantowerDir="<quantower>\TradingPlatform\v1.147.4\bin"
```

(Adjust the version subfolder to your Quantower install.)

Requires the **.NET 10 SDK** (the AddOn targets `net10.0-windows`, matching Quantower's .NET 10
runtime). Use `dotnet`, not `msbuild` — VS msbuild resolves an older SDK that can't target
`net10.0-windows`.

Output: **`bin/Release/`**.

---

