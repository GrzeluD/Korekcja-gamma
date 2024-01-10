#include "pch.h"
#include <cmath>
// Przyk³ad funkcji w C


extern "C" void vectorPow(double tablicaWykladnikow[8], int tablicaBazowa[8], double wynik[8]) {
    for (int i = 0; i < 8; ++i) {
        wynik[i] = pow(tablicaBazowa[i], tablicaWykladnikow[i]);
    }
}