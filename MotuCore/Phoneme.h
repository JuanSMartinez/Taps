//
//  Phoneme.h
//  ReadCSV
//
//  Created by Juan Sebastian Martinez on 10/28/17.
//  Copyright © 2017 Juan Sebastian Martinez. All rights reserved.
//

#ifndef Phoneme_h
#define Phoneme_h
#endif /* Phoneme_h */

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <map>

typedef struct{
    int rows;
    float* matrix;
}phonemeData;

class Phoneme{
public:
    Phoneme(int code);
    ~Phoneme();
    float valueAt(int i, int j);
    int getPhonemeCode();
    size_t getNumberOfRows();
	int getPhonemeDuration();
private:
    int phonemeCode;
	int duration;
    phonemeData* data;
    std::string getPathToDataFile();
    void initializeData();
};
