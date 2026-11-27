/* curves.c - the C part of the native library */
#include "curves.h"

double cr_age_factor(int age)
{
    if (age <= 17) return 0.85;
    if (age <= 21) return 1.10;
    if (age <= 25) return 1.15;
    if (age <= 28) return 1.05;
    switch (age) {
        case 29: return 0.90;
        case 30: return 0.75;
        case 31: return 0.60;
        case 32: return 0.45;
        case 33: return 0.32;
        case 34: return 0.22;
        default: return 0.12;
    }
}

double cr_potential_weight(int age)
{
    if (age <= 18) return 0.75;
    if (age <= 20) return 0.60;
    if (age <= 22) return 0.40;
    if (age <= 24) return 0.20;
    return 0.0;
}

double cr_contract_factor(int years_left)
{
    if (years_left <= 0) return 0.35;
    if (years_left == 1) return 0.70;
    if (years_left == 2) return 0.90;
    return 1.0;
}
