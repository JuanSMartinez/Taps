#pragma once
#include "MotuCore.h"

Handlers::MotuPlayer* player;

void AsyncSinePlay(void*) 
{
	PaError initResult = Pa_Initialize();
	if (initResult != paNoError) goto error;
	if (player->isUsingMotu())
	{
		if (!player->setMOTU()) goto error;
	}
	else
	{
		if (!player->setDefaultOutput()) goto error;
	}

	if (player->openStream()) 
	{
		if (player->startStream())
		{
			Pa_Sleep(2000);
			player->stopStream();
		}
		player->closeStream();
	}
	player->restartControlVariables();
	Pa_Terminate();
error:
	_endthread();
}

void AsyncPhonemePlay(void*)
{
	PaError initResult = Pa_Initialize();
	if (initResult != paNoError) goto error;
	if (player->isUsingMotu())
	{
		if (!player->setMOTU()) goto error;
	}
	else
	{
		if (!player->setDefaultOutput()) goto error;
	}
	if (player->openStream())
	{
		if (player->startStream())
		{
			while (player->isStreamActive()) ;
			player->stopStream();
		}
		player->closeStream();
	}
	player->restartControlVariables();
	Pa_Terminate();
error:
	_endthread();
}

void AsyncMatrixPlay(void*)
{
	PaError initResult = Pa_Initialize();
	if (initResult != paNoError) goto error;
	if (player->isUsingMotu())
	{
		if (!player->setMOTU()) goto error;
	}
	else
	{
		if (!player->setDefaultOutput()) goto error;
	}
	if (player->openStream())
	{
		if (player->startStream())
		{
			while (player->isStreamActive());
			player->stopStream();
		}
		player->closeStream();
	}
	player->restartControlVariables();
	Pa_Terminate();
error:
	_endthread();
}


/*Test a sine wave in all 24 channels*/
DLLEXPORT int testPlay()
{
	if (!player->isPlaying()) {
		player->changePlayMode(Handlers::MotuPlayer::sine);
		_beginthread(AsyncSinePlay, 0, NULL);
		return 0;
	}
	else return -1;
}

DLLEXPORT int play(int phonemeCode) 
{
	if (!player->isPlaying()) {
		player->setPhonemeIndex(phonemeCode);
		player->changePlayMode(Handlers::MotuPlayer::phoneme);
		_beginthread(AsyncPhonemePlay, 0, NULL);
		return 0;
	}
	else return -1;
}

//Play a specific matrix 
DLLEXPORT int playMatrix(float* matrix, int width, int height)
{
	if (!player->isPlaying()) {
		player->changePlayMode(Handlers::MotuPlayer::matrix);
		player->setArbitraryMatrixParameters(matrix, width, height);
		_beginthread(AsyncMatrixPlay, 0, NULL);

		return 0;
	}
	else return -1;
}

/*Create necessary data structures*/
DLLEXPORT void createStructures()
{
	player = new Handlers::MotuPlayer();
}

//Return the log code
DLLEXPORT int getLogCode()
{
	return 0;
}

//Is the device playing
DLLEXPORT bool isPlaying()
{
	return player->isPlaying();
}

//Use motu as the playback device
DLLEXPORT void useMotu()
{
	player->useMotu();
}

//Use the default device as the playback device
DLLEXPORT void useDefaultOutput()
{
	player->useDefault();
}

