# PVRSAIndicator

Quantower C# overlay indicator implementing the PVRSA context pack (PVSRA vector candles, EMA stack + cloud, floor pivots, YDay/LWeek, ADR/AWR/AMR/RD/RW, sessions, Psy Hi/Lo, DST table, vector candle zones).

**Product name is PVRSAIndicator.** Do not brand the chart as "PVRSA".

## Install

1. Copy `TradingPlatform.BusinessLayer.dll` from a Quantower install (typically next to `Quantower.exe`, or under the platform `bin` folder) so the project can compile. Either:
   - set MSBuild property `QuantowerDir` to that folder, or
   - put the DLL beside this `.csproj` and adjust the hint path.
2. Build:

```bash
msbuild PVRSAIndicator.csproj /p:Configuration=Release /p:QuantowerDir="C:\Path\To\Quantower"
```

3. Copy `PVRSAIndicator.dll` into the Quantower **Indicators** folder (Quantower looks there for custom indicators).
4. Restart Quantower (or reload scripts) and add **PVRSAIndicator** to a price chart.

## Notes

- Overlay only (`SeparateWindow = false`). No TDI, countdown, news, or trade-entry logic.
- `toPips` uses `Symbol.TickSize` as point size (UNKNOWN vs TradingView — no 10× multiplier).
- Alerts are named `Core.Instance.Loggers.Log(..., LoggingLevel.System)` messages. Default frequency is **OnBarClose**.
- Frankfurt session defaults **off**.
- Pine bugs fixed: 50% RD mid = (High50+Low50)/2; RD/RW currency labels use the **full** range; DST table says **April** (not "Arpil").
