// market_value.cpp - the C++ part of the native library.
// C# calls cr_market_value() through P/Invoke. It uses the C helpers from curves.c.
#include <algorithm>
#include <cmath>
#include <string>
#include <unordered_map>

#include "curves.h"

#if defined(_WIN32)
#define CR_API extern "C" __declspec(dllexport)
#else
#define CR_API extern "C" __attribute__((visibility("default")))
#endif

static double position_multiplier(const char* position)
{
    static const std::unordered_map<std::string, double> table = {
        {"ST", 1.20}, {"CF", 1.15}, {"LW", 1.10}, {"RW", 1.10}, {"CAM", 1.10},
        {"LM", 1.00}, {"RM", 1.00}, {"CM", 1.00}, {"CDM", 0.95}, {"CB", 0.95},
        {"LB", 0.90}, {"RB", 0.90}, {"GK", 0.75}
    };
    auto it = table.find(position ? position : "");
    return it == table.end() ? 1.0 : it->second;
}

// Market value in euros, rounded to the nearest 10,000.
CR_API double cr_market_value(int overall, int potential, int age,
                              const char* position, int contract_years_left, double form)
{
    const double base = 26700.0 * std::exp(0.17 * (overall - 40));

    const double effective = overall + std::max(0, potential - overall) * cr_potential_weight(age);
    const double rated = 26700.0 * std::exp(0.17 * (effective - 40));

    double value = std::max(base, rated);
    value *= cr_age_factor(age);
    value *= position_multiplier(position);
    value *= cr_contract_factor(contract_years_left);
    value *= form;

    // nearbyint rounds half to even, the same as C#'s Math.Round
    return std::nearbyint(value / 10000.0) * 10000.0;
}

// C# calls this first to find out whether the library loaded.
CR_API int cr_version()
{
    return 1;
}
