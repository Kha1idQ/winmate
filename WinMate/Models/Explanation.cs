namespace WinMate.Models;

// The four written parts of an "Explain this" card. The other three questions
// (needs admin? needs restart? how to undo?) are derived from the tweak itself,
// so they can never drift out of sync with what the tweak actually does.
public record Explanation(
    string WhatEn, string WhatAr,   // what it changes
    string WhyEn, string WhyAr,     // why you might want it
    string PerfEn, string PerfAr,   // honest performance impact (incl. "no real effect")
    string RiskEn, string RiskAr);  // what could go wrong
