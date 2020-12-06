using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace VPlotter.GCode
{
  [PublicAPI]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public enum Code
  {
    Invalid = 0,

    G0_RapidMove = ('G' << 24) | 0,
    G1_LinearMove = ('G' << 24) | 1,
    G2_ClockwiseArcMove = ('G' << 24) | 2,
    G3_CounterClockwiseArcMove = ('G' << 24) | 3,
    G4_Dwell = ('G' << 24) | 4,
    G5_CubicBSplineMove = ('G' << 24) | 5,
    G6_DirectStepperMove = ('G' << 24) | 6,

    G10_Retract = ('G' << 24) | 10,
    G11_Recover = ('G' << 24) | 11,
    G12_CleanTheNozzle = ('G' << 24) | 12,

    G20_InchUnits = ('G' << 24) | 20,
    G21_MillimeterUnits = ('G' << 24) | 21,

    G90_AbsolutePositioning = ('G' << 24) | 90,
    G91_RelativePositioning = ('G' << 24) | 91,
    G92_SetPosition = ('G' << 24) | 92,
  }
}