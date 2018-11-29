//
//  Phoneme.cpp
//  ReadCSV
//
//  Created by Juan Sebastian Martinez on 10/28/17.
//  Copyright © 2017 Juan Sebastian Martinez. All rights reserved.
//

#define SAMPLE_RATE   (44100)

#include <stdio.h>
#include "Phoneme.h"
#include <Windows.h>

static std::map<int, std::string>phonemeList = { {0,"P"},{1,"T"},{2,"K"},{3,"B"},{4,"D"},{5,"G"},{6,"CH"},{7,"J"},{8,"F"},{9,"V"},{10,"TH"},{11,"DH"},{12,"S"},{13,"Z"},{14,"SH"},{15,"ZH"},{16,"H"},{17,"M"},{18,"N"},{19,"NG"},{20,"L"},{21,"R"},{22,"W"},{23,"Y"},{24,"AE"},{25,"AH"},{26,"OE"},{27,"EH"},{28,"ER"},{29,"IH"},{30,"EE"},{31,"UH"},{32,"OO"},{33,"UU"},{34,"AW"},{35,"AY"},{36,"I"},{37,"OW"},{38,"OY"},
													{39,"DH&UH"}, {40,"UH&N"}, {41,"T&UH"}, {42,"N&D"},{43,"S&T"}, {44,"IH&T"},{45,"IH&N"},{46,"IH&NG"},{47,"N&T"},{48,"Y&OO"} };

int Phoneme::getPhonemeCode(){
    return phonemeCode;
}

std::string Phoneme::getPathToDataFile(){
    std::string basePath = "";
	std::vector<wchar_t> pathBuf;
	DWORD copied = 0;
	do {
		pathBuf.resize(pathBuf.size() + MAX_PATH);
		copied = GetModuleFileNameW(0, &pathBuf.at(0), pathBuf.size());
	} while (copied >= pathBuf.size());

	pathBuf.resize(copied);
	std::string path(pathBuf.begin(), pathBuf.end());
	std::string::size_type pos = std::string(path).find_last_of("\\/");
	basePath = std::string(path).substr(0, pos) + "\\Phonemes\\";
    return basePath + phonemeList[phonemeCode] + ".csv";
}

void Phoneme::initializeData(){

	data = new phonemeData;

    //Read the corresponding csv file for the phoneme
    std::string filePath = getPathToDataFile();
    std::vector<std::string> vector;
    std::string line;
    std::ifstream file;
    file.open(filePath);
    while(file.good()){
        getline(file,line);
        vector.push_back(line);
    }
    file.close();
    
    //Allocate memory for the data structure
    size_t rows = vector.size();
    int cols = 24;
    data->matrix = (float*)calloc(rows*cols, sizeof(float));
	//data->matrix = (float*)malloc(rows*cols*sizeof(float));
    data->rows = (int)rows;
	duration = ((float)rows / SAMPLE_RATE) * 1000;
    
    //Read the matrix
    for(int i = 0; i < rows; i++){
        std::string row = vector[i];
        size_t pos = row.find(",");
        std::string value;
        int j = 0;
        while(pos != std::string::npos){
            value = row.substr(0,pos);
            row.erase(0, pos + 1);
            data->matrix[i*24 + j] = atof(value.c_str());
            j++;
            pos = row.find(",");
        }
        data->matrix[i*24 + j] = atof(row.c_str());
    }
}

float Phoneme::valueAt(int i, int j){
	float result = data->matrix[i * 24 + j];
    return result;
}

size_t Phoneme::getNumberOfRows(){
    return data->rows;
}

int Phoneme::getPhonemeDuration() {
	return duration;
}

void Phoneme::setCode(int code)
{
	phonemeCode = code;
}

Phoneme::Phoneme(int code){
    phonemeCode = code;
    initializeData();
}

Phoneme::~Phoneme(){
    free(data->matrix);
    delete data;
}


