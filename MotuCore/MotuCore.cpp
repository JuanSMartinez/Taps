#pragma once
#include "MotuCore.h"

Handlers::MotuPlayer* player;
std::mutex* mutex;

void AsyncSinePlay(void*) 
{
	if (mutex->try_lock())
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
		mutex->unlock();
		_endthread();
	}
	else
		_endthread();
	
error:
	mutex->unlock();
	Pa_Terminate();
	_endthread();
}

void AsyncPhonemePlay(void*)
{
	if (mutex->try_lock())
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
		mutex->unlock();
		_endthread();
	}
	else
		_endthread();
error:
	mutex->unlock();
	Pa_Terminate();
	_endthread();
}

void AsyncMatrixPlay(void*)
{
	if (mutex->try_lock())
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
		mutex->unlock();
		_endthread();
	}
	else
		_endthread();
error:
	Pa_Terminate();
	mutex->unlock();
	_endthread();
}


/*Test a sine wave in all 24 channels*/
DLLEXPORT int testPlay()
{
	player->changePlayMode(Handlers::MotuPlayer::sine);
	_beginthread(AsyncSinePlay, 0, NULL);
	return 0;
}

DLLEXPORT int play(int phonemeCode) 
{
	player->setPhonemeIndex(phonemeCode);
	player->changePlayMode(Handlers::MotuPlayer::phoneme);
	_beginthread(AsyncPhonemePlay, 0, NULL);
	return 0;
}

//Play a specific matrix 
DLLEXPORT int playMatrix(float* matrix, int width, int height)
{
	player->changePlayMode(Handlers::MotuPlayer::matrix);
	player->setArbitraryMatrixParameters(matrix, width, height);
	_beginthread(AsyncMatrixPlay, 0, NULL);
}

/*Create necessary data structures*/
DLLEXPORT void createStructures()
{
	player = new Handlers::MotuPlayer();
	mutex = new std::mutex;
}

//Return the log code
DLLEXPORT int getLogCode()
{
	return 0;
}

//Is the device playing
DLLEXPORT bool isMotuPlaying()
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


