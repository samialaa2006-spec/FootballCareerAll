/* curves.h - small C helpers used by the C++ market value code */
#ifndef CURVES_H
#define CURVES_H

#ifdef __cplusplus
extern "C" {
#endif

/* Value multiplier for a player's age (peaks at 22-25, collapses after 32). */
double cr_age_factor(int age);

/* How much of a young player's potential counts towards his value (0.0 - 0.75). */
double cr_potential_weight(int age);

/* Value multiplier for the years left on the contract. */
double cr_contract_factor(int years_left);

#ifdef __cplusplus
}
#endif

#endif /* CURVES_H */
