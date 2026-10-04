# PVRSAIndicator (v1.0.0)

*For general information, see the root README.*

Quantower C# overlay indicator implementing the PVRSA context pack (PVSRA vector candles, EMA stack + cloud, floor pivots, YDay/LWeek, ADR/AWR/AMR/RD/RW, sessions, Psy Hi/Lo, DST table, vector candle zones).


## Install

To ensure the project compiles, you must provide the Quantower Business Layer DLL.

1. Copy `TradingPlatform.BusinessLayer.dll` from your Quantower installation (usually located in the `bin` folder next to `Quantower.exe`). 
   - *Note: If you have trouble finding it, search your drive for the DLL file.*
   
   You can provide this in one of two ways:
   - Set the MSBuild property `QuantowerDir` to that folder.
   - Place the DLL in the same directory as this `.csproj` and adjust the hint path in the project file.

2. Build the project:

```bash
msbuild PVRSAIndicator.csproj /p:Configuration=Release /p:QuantowerDir="C:\Path\To\Quantower"
```

3. Copy the resulting `PVRSAIndicator.dll` into your Quantower **Indicators** folder.
4. Restart Quantower (or reload scripts) and add **PVRSAIndicator** to a price chart.

## Notes

- `toPips` uses `Symbol.TickSize`.
- Alerts are named `Core.Instance.Loggers.Log(..., LoggingLevel.System)` messages. Default frequency is **OnBarClose**.
- Frankfurt session defaults **off**.
